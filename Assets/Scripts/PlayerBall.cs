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

public class PlayerBall : Ball
{
    [SerializeField]
    Material TransparentMaterial;

    private Material _standartMaterial;
    private MeshRenderer _meshRenderer;

    private Transform _controllerTransform;

    public bool IsGrabed;
    private bool _setForceReady;

    public float ForceConst = 100;

    private Network _networkScript;
    private CameraRaycast _cameraScript;

    Transform _net1Transform;
    Transform _net2Transform;

    public Vector3 netPos;
    public float magnitude;

    private float _zone3pt = 7f;
    public bool isZone3pt;

    private bool _setIK;

    private float _forceOffset = 0;

    public Vector3 startToFlyPosition;

    private PointCam _pointCam;
    private BallCustomize _ballCustomize;

    void Awake()
    {
        _ballCustomize = FindObjectOfType<BallCustomize>();
        _networkScript = FindObjectOfType<Network>();
        _pointCam = GameObject.FindObjectOfType<PointCam>();

        _cameraScript = GameObject.Find("Main Camera").GetComponent<CameraRaycast>();
        _net1Transform = GameObject.Find("net").transform;
        _net2Transform = GameObject.Find("net (1)").transform;

        _bouncesSource = GetComponent<AudioSource>();
        _rigidBody = GetComponent<Rigidbody>();

        _meshRenderer = GetComponent<MeshRenderer>();
        _standartMaterial = _meshRenderer.material;

        _transform = transform;
        _controllerTransform = GameObject.Find("PlayerController").transform;
    }

    void Update()
    {
        Vector3 ControllerPos = new Vector3();
        PlayerTransform ballTransformData = new PlayerTransform();

        if(_controllerTransform != null)
            ControllerPos = new Vector3(_controllerTransform.position.x, 0, _controllerTransform.position.z);
        else ControllerPos = Vector3.zero;

        magnitude = (netPos - ControllerPos).magnitude;

        if(CameraRaycast.isBoard1)
            netPos = new Vector3(_net1Transform.position.x, 0, _net1Transform.position.z);
        else if(CameraRaycast.isBoard2)
            netPos = new Vector3(_net2Transform.position.x, 0, _net2Transform.position.z);

        if(magnitude >= _zone3pt)
            isZone3pt = true;
        else
            isZone3pt = false;

        if(IsGrabed)
        {
            Vector3 BallRotation = new Vector3(_controllerTransform.rotation.eulerAngles.x, Camera.main.transform.rotation.eulerAngles.y + 90, _controllerTransform.rotation.eulerAngles.z);

            _transform.position = Vector3.Lerp(_transform.position, CameraRaycast.cameraTransform.position + (CameraRaycast.cameraTransform.forward * 0.5f), 20 * Time.deltaTime);
            _transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(BallRotation), 20 * Time.deltaTime);

            if(Network.inRoom)
            {
                if(_lastPosition != _transform.position && _lastRotation.eulerAngles != _transform.rotation.eulerAngles)
                {
                    var ballPosition = _transform.position;
                    var ballRotation = _transform.rotation.eulerAngles;

                    ballTransformData.X = ballPosition.x;
                    ballTransformData.Y = ballPosition.y;
                    ballTransformData.Z = ballPosition.z;

                    ballTransformData.RotX = ballRotation.x;
                    ballTransformData.RotY = ballRotation.y;
                    ballTransformData.RotZ = ballRotation.z;

                    _networkScript.SendServerData("ServerOnPlayerMoving", ballTransformData);
                }
            }

            _lastPosition = _transform.position;
            _lastRotation = _transform.rotation;
        }
    }

    public void SetBallOutlook(Material newMaterial) => _standartMaterial = newMaterial;

    public void PhysicEnable()
    {
        Force throwForceData = new Force();
        Vector3 throwPosition = _transform.position;
        Vector3 throwRotation = _transform.rotation.eulerAngles;

        parketHitCount = 0;
        startToFlyPosition = new Vector3(transform.position.x, 0, transform.position.z);

        _pointCam.OnBallThrow(this);

        if(c_chekBallHigh == null)
            c_chekBallHigh = StartCoroutine(ChekHigh());
        else
        {
            StopCoroutine(c_chekBallHigh);
            c_chekBallHigh = null;
        }

        if(c_changeForceOffset != null)
        {
            StopCoroutine(c_changeForceOffset);
            c_changeForceOffset = null;
        }

        _meshRenderer.material = _standartMaterial;

        if(_setIK)
        {
            if(c_ballOnAir == null)
                c_ballOnAir = StartCoroutine(WaitingBallOnAir());
            else
            {
                StopCoroutine(c_ballOnAir);
                c_ballOnAir = StartCoroutine(WaitingBallOnAir());
            }

            _rigidBody.isKinematic = false;
            _rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            _rigidBody.detectCollisions = true;
            _setIK = false;
        }

        IsGrabed = false;

        Vector3 Force = new Vector3();

        if(_setForceReady)
        {
            Force = (_cameraScript.shootPoint.position - _transform.position) * (ForceConst + _forceOffset);
            var torque = -Camera.main.transform.right * Random.Range(0.2f, 1);

            _forceOffset = 0;
            _rigidBody.AddForce(Force);

            _setForceReady = false;
            _rigidBody.AddTorque(torque);

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
        _pointCam.OnBallGrab(transform);

        StopCheckBallHight();

        _ballOnParket = false;
        ballCorrectHigh = false;

        // _guiScript.SetActivePointCam(false, null);

        _meshRenderer.material = TransparentMaterial;

        _rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        _rigidBody.isKinematic = true;
        _setForceReady = true;
        _rigidBody.detectCollisions = false;
        IsGrabed = true;

        _setIK = true;
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        if(collision.transform.tag == "Parket")
        {
            _cameraScript.ableToShowPointCam = false;
            _pointCam.OnBallThrow(this);

            parketHitCount++;
            if(parketHitCount == 1)
            {
                if(c_ballOnAir != null)
                {
                    StopCoroutine(c_ballOnAir);
                    c_ballOnAir = null;
                }

                _ballOnParket = true;
            }

        }
    }

    public void BallSetArcadePosition()
    {
        var CameraRotation = CameraRaycast.cameraTransform.localRotation;

        _rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        _rigidBody.isKinematic = true;

        Vector3 target = _net1Transform.position - PlayerController.controllerTransform.position;
        CameraRotation = Quaternion.LookRotation(target);

        PlayerController.controllerTransform.rotation = Quaternion.Euler(new Vector3(0, CameraRotation.eulerAngles.y, 0));
        CameraRaycast.cameraTransform.localRotation = Quaternion.Euler(new Vector3(CameraRotation.eulerAngles.x, 0, CameraRotation.eulerAngles.z));

        _transform.position = CameraRaycast.cameraTransform.position + (CameraRaycast.cameraTransform.forward * 0.9f);
    }

    public void SetForceOffset(bool _isStart)
    {
        if(_isStart == true)
        {
            if(c_changeForceOffset != null)
            {
                StopCoroutine(c_changeForceOffset);
                c_changeForceOffset = null;
            }
            c_changeForceOffset = StartCoroutine(ChangeForceOffset());
        }
        else
        {
            if(c_changeForceOffset != null)
            {
                StopCoroutine(c_changeForceOffset);
                c_changeForceOffset = null;
            }
            _meshRenderer.material = TransparentMaterial;
            _forceOffset = 0;
        }
    }

    private IEnumerator ChangeForceOffset()
    {
        int RandSpeed;
        
        if(PlayerController.netDistance < 6.9f)
        {
            RandSpeed = Random.Range(2, 7);
        }
        else
        {
            RandSpeed = Random.Range(5, 7);
        }

        int ForceOffsetMax;

        if(PlayerController.controllerTransform.position.x != 8.35f)
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
