using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheSanctuary;

//  скрипт описывает логику поведения игрового мяча.
//
//  coroutine С_BallOnAir проверяет, находится ли мяч в полёте.
//  если за определенное время мяч не каснется паркета - дать ему импульс,
//  т.к. скорее всего он остался на душке кольца;
//
//  coroutine C_ChekBallHigh - высоту мяча относительно триггера кольца,
//  на случай, если мяч был заброшен снизу, или прошлолся по
//  касательной после броска с большой дистанции;
//  
//  coroutine C_ChangeForceOffset - задаёт плавное изменение погрешности
//  броска, т.к. без этого мяч практически всегда попадает в кольцо

public class BallLogic : MonoBehaviour
{
    [SerializeField]
    List<AudioClip> BouncesSound = new List<AudioClip>();
    [SerializeField]
    Material TransparentMaterial;

    private Material _standartMaterial;
    MeshRenderer _meshRenderer;

    private Transform _ballTransform;
    private Transform _controllerTransform;

    private Rigidbody _ballRigidBody;
    public bool IsGrabed;
    private bool _setForceReady;
    private AudioSource _bouncesSource;

    public float ForceConst = 100;

    private GUI _guiScript;
    private Network _networkScript;
    private CameraRaycast _cameraScript;

    public bool OnAir;
    public bool itsPoint;
    public int ParketHitCount = 0;

    Transform _net1Transform;
    Transform _net2Transform;

    public Vector3 NetPos;
    public float Maginude;

    private float _zone3pt = 7f;
    public bool isZone3pt;

    private bool _setIK;

    private Coroutine C_BallOnAir;
    private Coroutine C_ChekBallHigh;
    private Coroutine C_ChangeForceOffset;

    public bool BallCorrectHigh;

    private bool _ballOnParket;
    private float _forceOffset = 0;

    public Vector3 StartToFlyPosition;

    private PointCam _pointCamScript;
    private BallCustomize _ballCustomize;

    private Vector3 _lastPosition;
    private Vector3 _lastRotation;

    void Awake()
    {
        _ballCustomize = FindObjectOfType<BallCustomize>();
        _networkScript = FindObjectOfType<Network>();
        _pointCamScript = GameObject.FindObjectOfType<PointCam>();

        _cameraScript = GameObject.Find("Main Camera").GetComponent<CameraRaycast>();
        _guiScript = GameObject.Find("Canvas").GetComponent<GUI>();
        _net1Transform = GameObject.Find("net").transform;
        _net2Transform = GameObject.Find("net (1)").transform;

        _bouncesSource = GetComponent<AudioSource>();
        _ballRigidBody = GetComponent<Rigidbody>();

        _meshRenderer = GetComponent<MeshRenderer>();
        _standartMaterial = _meshRenderer.material;

        _ballTransform = transform;
        _controllerTransform = GameObject.Find("PlayerController").transform;

        GameObject.FindObjectOfType<ScoreTrigger>().OnBallInit();
    }

    void Update()
    {
        Vector3 ControllerPos = new Vector3();
        PlayerTransform ballTransformData = new PlayerTransform();

        if(_controllerTransform != null)
            ControllerPos = new Vector3(_controllerTransform.position.x, 0, _controllerTransform.position.z);
        else ControllerPos = Vector3.zero;

        Maginude = (NetPos - ControllerPos).magnitude;

        if(CameraRaycast.IsBoard1)
            NetPos = new Vector3(_net1Transform.position.x, 0, _net1Transform.position.z);
        else if(CameraRaycast.IsBoard2)
            NetPos = new Vector3(_net2Transform.position.x, 0, _net2Transform.position.z);

        if(Maginude >= _zone3pt)
            isZone3pt = true;
        else
            isZone3pt = false;

        if(IsGrabed)
        {
            Vector3 BallRotation = new Vector3(_controllerTransform.rotation.eulerAngles.x, Camera.main.transform.rotation.eulerAngles.y + 90, _controllerTransform.rotation.eulerAngles.z);

            _ballTransform.position = Vector3.Lerp(_ballTransform.position, CameraRaycast.CameraTransform.position + (CameraRaycast.CameraTransform.forward * 0.5f), 20 * Time.deltaTime);
            _ballTransform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(BallRotation), 20 * Time.deltaTime);

            if(Network.inRoom)
            {
                if(_lastPosition != _ballTransform.position && _lastRotation != _ballTransform.rotation.eulerAngles)
                {
                    var ballPosition = _ballTransform.position;
                    var ballRotation = _ballTransform.rotation.eulerAngles;

                    ballTransformData.X = ballPosition.x;
                    ballTransformData.Y = ballPosition.y;
                    ballTransformData.Z = ballPosition.z;

                    ballTransformData.RotX = ballRotation.x;
                    ballTransformData.RotY = ballRotation.y;
                    ballTransformData.RotZ = ballRotation.z;

                    _networkScript.SendServerData("ServerOnPlayerMoving", ballTransformData);
                }
            }

            _lastPosition = _ballTransform.position;
            _lastRotation = _ballTransform.rotation.eulerAngles;
        }
    }

    public void SetBallOutlook(Material newMaterial) => _standartMaterial = newMaterial;

    public void PhysicEnable()
    {
        Force throwForceData = new Force();
        Vector3 throwPosition = _ballTransform.position;
        Vector3 throwRotation = _ballTransform.rotation.eulerAngles;

        ParketHitCount = 0;
        StartToFlyPosition = new Vector3(transform.position.x, 0, transform.position.z);
        _pointCamScript.OnBallThrow(transform.name);

        if(C_ChekBallHigh == null)
            C_ChekBallHigh = StartCoroutine(ChekHigh());
        else
        {
            StopCoroutine(C_ChekBallHigh);
            C_ChekBallHigh = null;
        }

        if(_cameraScript.AbleToShowPointCam)
            _guiScript.SetActivePointCam(true, transform);

        if(C_ChangeForceOffset != null)
        {
            StopCoroutine(C_ChangeForceOffset);
            C_ChangeForceOffset = null;
        }




        _meshRenderer.material = _standartMaterial;

        if(_setIK)
        {
            if(C_BallOnAir == null)
                C_BallOnAir = StartCoroutine(WaitingBallOnAir());
            else
            {
                StopCoroutine(C_BallOnAir);
                C_BallOnAir = StartCoroutine(WaitingBallOnAir());
            }

            _ballRigidBody.isKinematic = false;
            _ballRigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            _ballRigidBody.detectCollisions = true;
            _setIK = false;
        }

        IsGrabed = false;

        Vector3 Force = new Vector3();

        if(_setForceReady)
        {
            Force = (_cameraScript.ShootPoint.position - _ballTransform.position) * (ForceConst + _forceOffset);
            var torque = -Camera.main.transform.right * Random.Range(0.2f, 1);

            _forceOffset = 0;
            _ballRigidBody.AddForce(Force);

            _setForceReady = false;
            _ballRigidBody.AddTorque(torque);

            if(Network.inRoom)
            {
                throwForceData.posX = throwPosition.x;
                throwForceData.posY = throwPosition.y;
                throwForceData.posZ = throwPosition.z;

                throwForceData.X = Force.x;
                throwForceData.Y = Force.y;
                throwForceData.Z = Force.z;

                throwForceData.rotX = throwRotation.x;
                throwForceData.rotY = throwRotation.y;
                throwForceData.rotZ = throwRotation.z;

                throwForceData.torqueX = torque.x;
                throwForceData.torqueY = torque.y;
                throwForceData.torqueZ = torque.z;

                _networkScript.SendServerData("ServerOnBallThrowning", throwForceData);
            }
        }
    }

    public void PhysicDisable()
    {
        _pointCamScript.OnBallGrab(transform);

        if(C_ChekBallHigh != null)
        {
            StopCoroutine(C_ChekBallHigh);
            C_ChekBallHigh = null;
        }

        _ballOnParket = false;
        BallCorrectHigh = false;

        _guiScript.SetActivePointCam(false, null);

        _meshRenderer.material = TransparentMaterial;

        _ballRigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        _ballRigidBody.isKinematic = true;
        _setForceReady = true;
        _ballRigidBody.detectCollisions = false;
        IsGrabed = true;

        _setIK = true;

        CameraRaycast.CurrentBall = transform.name;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag == "Parket")
        {
            _cameraScript.AbleToShowPointCam = false;
            GameObject.FindObjectOfType<PointCam>().OnBallThrow(this.transform.name);

            ParketHitCount++;
            if(ParketHitCount == 1)
            {
                if(C_BallOnAir != null)
                {
                    StopCoroutine(C_BallOnAir);
                    C_BallOnAir = null;
                }

                _ballOnParket = true;
            }

        }

        if (collision.relativeVelocity.magnitude > 0.1f)
        {
            _bouncesSource.clip = BouncesSound[Random.Range(0, BouncesSound.Count)];
            _bouncesSource.Play();
        }
    }

    public void BallSetArcadePosition()
    {
        var CameraRotation = CameraRaycast.CameraTransform.localRotation;

        _ballRigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        _ballRigidBody.isKinematic = true;

        Vector3 target = _net1Transform.position - PlayerController.ControllerTransform.position;
        CameraRotation = Quaternion.LookRotation(target);

        PlayerController.ControllerTransform.rotation = Quaternion.Euler(new Vector3(0, CameraRotation.eulerAngles.y, 0));
        CameraRaycast.CameraTransform.localRotation = Quaternion.Euler(new Vector3(CameraRotation.eulerAngles.x, 0, CameraRotation.eulerAngles.z));

        _ballTransform.position = CameraRaycast.CameraTransform.position + (CameraRaycast.CameraTransform.forward * 0.9f);
    }

    public void SetForceOffset(bool _isStart)
    {
        if(_isStart == true)
        {
            if(C_ChangeForceOffset != null)
            {
                StopCoroutine(C_ChangeForceOffset);
                C_ChangeForceOffset = null;
            }
            C_ChangeForceOffset = StartCoroutine(ChangeForceOffset());
        }
        else
        {
            if(C_ChangeForceOffset != null)
            {
                StopCoroutine(C_ChangeForceOffset);
                C_ChangeForceOffset = null;
            }
            _meshRenderer.material = TransparentMaterial;
            _forceOffset = 0;
        }
    }

    private IEnumerator WaitingBallOnAir()
    {
        int count = 0;

        while(_ballOnParket == false)
        {
            yield return new WaitForSeconds(1);
            count++;
            if(count == 10)
            {
                _ballRigidBody.AddForce(new Vector3(2, 2, 0), ForceMode.Impulse);
                break;
            }
        }
        yield break;
    }

    public void StopCheckBallHight()
    {
        if(C_ChekBallHigh != null)
        {
            StopCoroutine(C_ChekBallHigh);
            C_ChekBallHigh = null;
        }
    }

    public IEnumerator ChekHigh()
    {
        while(true)
        {
            yield return null;
            if(_ballTransform.position.y >= 2.8f)
            {
                BallCorrectHigh = true;
                break;
            }
        }
        yield break;
    }

    private IEnumerator ChangeForceOffset()
    {
        int RandSpeed;
        if(PlayerController.NetDistance < 6.9f)
        {
            RandSpeed = Random.Range(2, 7);
        }
        else
        {
            RandSpeed = Random.Range(5, 7);
        }

        int ForceOffsetMax;

        if(PlayerController.ControllerTransform.position.x != 8.35f)
            ForceOffsetMax = 2;
        else
            ForceOffsetMax = 5;

        Color StartColor = Color.yellow;
        bool LowLevelLimit = false;

        yield return null;
        while(true)
        {
            Color CurrentColor = _meshRenderer.material.GetColor("Color_e7a44bfbee1a44c9aa58b1516c7eb71b");

            if(!LowLevelLimit)
            {
                CurrentColor = Color.Lerp(CurrentColor, Color.red, RandSpeed * Time.deltaTime);
                _forceOffset = Mathf.Clamp(Mathf.Lerp(_forceOffset, ForceOffsetMax, RandSpeed * Time.deltaTime), 0, ForceOffsetMax);

                if(CurrentColor.r >= 0.95f)
                {
                    _forceOffset = 2;
                    LowLevelLimit = true;
                }
            }
            else
            {
                CurrentColor = Color.Lerp(CurrentColor, Color.green, RandSpeed * Time.deltaTime);
                _forceOffset = Mathf.Clamp(Mathf.Lerp(_forceOffset, 0, RandSpeed * Time.deltaTime), 0, ForceOffsetMax);

                if(CurrentColor.g >= 0.9f)
                {
                    _forceOffset = 0;
                    LowLevelLimit = false;
                }
            }

            _meshRenderer.material.SetColor("Color_e7a44bfbee1a44c9aa58b1516c7eb71b", CurrentColor);

            yield return null;
        }
    }
}
