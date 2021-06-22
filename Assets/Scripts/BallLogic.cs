using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheSanctuary;

public class BallLogic : MonoBehaviour
{
    [SerializeField]
    List<AudioClip> BouncesSound = new List<AudioClip>();
    [SerializeField]
    Material TransparentMaterial;

    public Material StandartMaterial;
    MeshRenderer MeshRenderer;

    private Transform BallTransform;
    Transform ControllerTransform;

    private Rigidbody BallRigidBody;
    public bool IsGrabed;
    private bool SetForceReady;
    private AudioSource BouncesSource;

    public float ForceConst = 100;

    // private NetworkGameRules GameRulesScript;
    private GUI GUIScript;
    private Network networkScript;
    private CameraRaycast CameraScript;
    private Arcade ArcadeScript;

    public bool OnAir;
    public bool itsPoint;
    public int ParketHitCount = 0;

    Transform Net1Transform;
    Transform Net2Transform;

    public Vector3 NetPos;
    public float Maginude;

    private float Zone3pt = 7f;
    public bool isZone3pt;

    private bool SetIK;

    private Coroutine BallOnAir;
    public Coroutine C_ChekBallHigh;

    public bool BallCorrectHigh;

    private bool BallOnParket;

    private float ForceOffset = 0;
    private Coroutine C_ChangeForceOffset;

    public Vector3 StartToFlyPosition;

    private PointCam PointCamScript;

    private Vector3 _lastPosition;
    private Vector3 _lastRotation;

    void Awake()
    {
        networkScript = FindObjectOfType<Network>();
        PointCamScript = GameObject.FindObjectOfType<PointCam>();

        CameraScript = GameObject.Find("Main Camera").GetComponent<CameraRaycast>();
        GUIScript = GameObject.Find("Canvas").GetComponent<GUI>();
        Net1Transform = GameObject.Find("net").transform;
        Net2Transform = GameObject.Find("net (1)").transform;
        //GameRulesScript = GameObject.Find("CANDYSHOP").GetComponent<NetworkGameRules>();
        ArcadeScript = GameObject.FindObjectOfType<Arcade>();

        BouncesSource = GetComponent<AudioSource>();
        BallRigidBody = GetComponent<Rigidbody>();

        MeshRenderer = GetComponent<MeshRenderer>();
        StandartMaterial = MeshRenderer.material;

        BallTransform = transform;
        ControllerTransform = GameObject.Find("PlayerController").transform;

        //GameObject.FindObjectOfType<NetworkManager>().OnBallInit();
        //GameObject.FindObjectOfType<PointCam>().OnBallInit();
        GameObject.FindObjectOfType<ScoreTrigger>().OnBallInit();
    }

    void Update()
    {
        Vector3 ControllerPos = new Vector3();
        PlayerTransform ballTransformData = new PlayerTransform();

        if(ControllerTransform != null) ControllerPos = new Vector3(ControllerTransform.position.x, 0, ControllerTransform.position.z);
        else ControllerPos = Vector3.zero;

        Maginude = (NetPos - ControllerPos).magnitude;
        if(CameraRaycast.IsBoard1)
        NetPos = new Vector3(Net1Transform.position.x, 0, Net1Transform.position.z);
        else if(CameraRaycast.IsBoard2)
        NetPos = new Vector3(Net2Transform.position.x, 0, Net2Transform.position.z);

        if(Maginude >= Zone3pt)
        {
            isZone3pt = true;
        }
        else
        {
            isZone3pt = false;
        }

        if (IsGrabed)
        {
            Vector3 BallRotation = new Vector3(ControllerTransform.rotation.eulerAngles.x, Camera.main.transform.rotation.eulerAngles.y + 90, ControllerTransform.rotation.eulerAngles.z);

            BallTransform.position = Vector3.Lerp(BallTransform.position, CameraRaycast.CameraTransform.position + (CameraRaycast.CameraTransform.forward * 0.5f), 20 * Time.deltaTime);
            BallTransform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(BallRotation), 20 * Time.deltaTime);

            if(Network.inRoom)
            {
                if(_lastPosition != BallTransform.position && _lastRotation != BallTransform.rotation.eulerAngles)
                {
                    var ballPosition = BallTransform.position;
                    var ballRotation = BallTransform.rotation.eulerAngles;

                    ballTransformData.X = ballPosition.x;
                    ballTransformData.Y = ballPosition.y;
                    ballTransformData.Z = ballPosition.z;

                    ballTransformData.RotX = ballRotation.x;
                    ballTransformData.RotY = ballRotation.y;
                    ballTransformData.RotZ = ballRotation.z;

                    networkScript.SendServerData("ServerOnPlayerMoving", ballTransformData);

                    _lastPosition = BallTransform.position;
                    _lastRotation = BallTransform.rotation.eulerAngles;
                }
            }
        }
    }

    public void PhysicEnable()
    {
        Force throwForceData = new Force();
        Vector3 throwPosition = BallTransform.position;
        Vector3 throwRotation = BallTransform.rotation.eulerAngles;

        ParketHitCount = 0;
        StartToFlyPosition = new Vector3(transform.position.x, 0, transform.position.z);
        PointCamScript.OnBallThrow(transform.name);

        if(C_ChekBallHigh == null)
        {
            C_ChekBallHigh = StartCoroutine(ChekBallHigh());
        }
        else
        {
            StopCoroutine(C_ChekBallHigh);
            C_ChekBallHigh = null;
        }

        // if(GameRulesScript != null && GameRulesScript.isGame)
        // {
        //     OnAir = true;
        //     if(GameRulesScript.is3pt && !isZone3pt)
        //     {
        //         GUIScript.ShowPopUpMessage("This point will not be counted", Color.red, 2);
        //     }
        // }

        if(CameraScript.AbleToShowPointCam)
        {
            GUIScript.SetActivePointCam(true, transform);
        }

        if(C_ChangeForceOffset != null)
        {
            StopCoroutine(C_ChangeForceOffset);
            C_ChangeForceOffset = null;
        }




        MeshRenderer.material = StandartMaterial;

        if(SetIK)
        {
            if(BallOnAir == null)
                BallOnAir = StartCoroutine(WaitingBallOnAir());
            else
            {
                StopCoroutine(BallOnAir);
                BallOnAir = StartCoroutine(WaitingBallOnAir());
            }

            BallRigidBody.isKinematic = false;
            BallRigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            BallRigidBody.detectCollisions = true;
            SetIK = false;
        }

        IsGrabed = false;

        Vector3 Force = new Vector3();

        if(SetForceReady)
        {
            Force = (CameraScript.ShootPoint.position - BallTransform.position) * (ForceConst + ForceOffset);
            var torque = -Camera.main.transform.right * Random.Range(0.2f, 1);

            ForceOffset = 0;
            BallRigidBody.AddForce(Force);

            SetForceReady = false;
            BallRigidBody.AddTorque(torque);

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

                networkScript.SendServerData("ServerOnBallThrowning", throwForceData);
            }
        }
    }

    public void PhysicDisable()
    {
        PointCamScript.OnBallGrab(transform);

        if(C_ChekBallHigh != null)
        {
            StopCoroutine(C_ChekBallHigh);
            C_ChekBallHigh = null;
        }

        BallOnParket = false;
        BallCorrectHigh = false;

        GUIScript.SetActivePointCam(false, null);

        MeshRenderer.material = TransparentMaterial;

        BallRigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        BallRigidBody.isKinematic = true;
        SetForceReady = true;
        BallRigidBody.detectCollisions = false;
        IsGrabed = true;

        SetIK = true;

        CameraRaycast.CurrentBall = transform.name;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag == "Parket")
        {
            CameraScript.AbleToShowPointCam = false;
            GameObject.FindObjectOfType<PointCam>().OnBallThrow(this.transform.name);

            ParketHitCount++;
            if(ParketHitCount == 1)
            {
                if(ArcadeScript.ArcadeMode)
                {
                    ArcadeScript.OnBallCollisionWithParket();
                }

                if(BallOnAir != null)
                {
                    StopCoroutine(BallOnAir);
                    BallOnAir = null;
                }

                BallOnParket = true;

                // if(GameRulesScript != null && (GameRulesScript.isGame == true && OnAir == true))
                // {
                //     var HitPoint = collision.contacts[0];
                //     // NetworkManager.SendShootResult(PlayerController.myID, itsPoint);
                //     if(itsPoint == false) GameRulesScript.SetMarkerVisable(new Vector3(HitPoint.point.x, 0, HitPoint.point.z));
                // }
            }

        }

        if (collision.relativeVelocity.magnitude > 0.1f)
        {
            BouncesSource.clip = BouncesSound[Random.Range(0, BouncesSound.Count)];
            BouncesSource.Play();
        }
    }

    public void BallSetArcadePosition()
    {
        var CameraRotation = CameraRaycast.CameraTransform.localRotation;

        BallRigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        BallRigidBody.isKinematic = true;

        Vector3 target = Net1Transform.position - PlayerController.ControllerTransform.position;
        CameraRotation = Quaternion.LookRotation(target);

        PlayerController.ControllerTransform.rotation = Quaternion.Euler(new Vector3(0, CameraRotation.eulerAngles.y, 0));
        CameraRaycast.CameraTransform.localRotation = Quaternion.Euler(new Vector3(CameraRotation.eulerAngles.x, 0, CameraRotation.eulerAngles.z));

        BallTransform.position = CameraRaycast.CameraTransform.position + (CameraRaycast.CameraTransform.forward * 0.9f);

        if(ArcadeScript.ArcadeMode == false)
        {
            BallRigidBody.isKinematic = false;
            BallRigidBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            BallRigidBody.detectCollisions = true;
            //BallRigidBody.AddForce(0, -0.01f, 0);
        }
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
            MeshRenderer.material = TransparentMaterial;
            ForceOffset = 0;
        }
    }

    private IEnumerator WaitingBallOnAir()
    {
        int count = 0;

        while(BallOnParket == false)
        {
            yield return new WaitForSeconds(1);
            count++;
            if(count == 10)
            {
                BallRigidBody.AddForce(new Vector3(2, 2, 0), ForceMode.Impulse);
                break;
            }
        }
        yield break;
    }

    public IEnumerator ChekBallHigh()
    {
        while(true)
        {
            yield return null;
            if(BallTransform.position.y >= 2.8f)
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
            Color CurrentColor = MeshRenderer.material.GetColor("Color_e7a44bfbee1a44c9aa58b1516c7eb71b");

            if(!LowLevelLimit)
            {
                CurrentColor = Color.Lerp(CurrentColor, Color.red, RandSpeed * Time.deltaTime);
                ForceOffset = Mathf.Clamp(Mathf.Lerp(ForceOffset, ForceOffsetMax, RandSpeed * Time.deltaTime), 0, ForceOffsetMax);

                if(CurrentColor.r >= 0.95f)
                {
                    ForceOffset = 2;
                    LowLevelLimit = true;
                }
            }
            else
            {
                CurrentColor = Color.Lerp(CurrentColor, Color.green, RandSpeed * Time.deltaTime);
                ForceOffset = Mathf.Clamp(Mathf.Lerp(ForceOffset, 0, RandSpeed * Time.deltaTime), 0, ForceOffsetMax);

                if(CurrentColor.g >= 0.9f)
                {
                    ForceOffset = 0;
                    LowLevelLimit = false;
                }
            }

            MeshRenderer.material.SetColor("Color_e7a44bfbee1a44c9aa58b1516c7eb71b", CurrentColor);

            yield return null;
        }
    }
}
