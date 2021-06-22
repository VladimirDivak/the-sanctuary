using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheSanctuary;
using System.Linq;

public class NetworkBall : MonoBehaviour
{
    [SerializeField]
    private List<AudioClip> BouncesSound = new List<AudioClip>();

    public string PlayerID;
    private PersonalData _playerData = new PersonalData();

    public string PlayerName => _playerData.login;

    private AudioSource _bouncesSource;

    public Transform NetworkBallTransform;
    public Rigidbody NetworkBallRB;

    public int ParketHitCount = 0;
    public bool OnAir;
    public bool itsPoint;

    private bool Grabing;

    private Vector3 LastPos;
    private Quaternion LastRot;

    private Coroutine BallOnAir;
    private Coroutine BallGrabing;
    public Coroutine C_ChekBallHigh;

    private bool BallOnParket;
    public bool BallCorrectHigh;

    private void Start()
    {
        NetworkBallTransform = transform;
        _bouncesSource = GetComponent<AudioSource>();
        NetworkBallRB = GetComponent<Rigidbody>();

        Network.OnBallMovingEvent += OnBallMoving;
        Network.OnBallThrowingEvent += OnBallThrowing;

        FindObjectOfType<GameManager>().ClothColliders.Add(new ClothSphereColliderPair(GetComponent<SphereCollider>(), GetComponent<SphereCollider>()));
    }

    public void SetNetworkBallData(string id, PersonalData data, bool readyStatus)
    {
        PlayerID = id;
        transform.name = PlayerID;

        _playerData = data;

        Color baseColor;
        Color linesColor;

        Material material = GetComponent<Renderer>().sharedMaterial;

        Debug.Log(_playerData.baseColor);
        Debug.Log(_playerData.linesColor);

        Debug.Log(ColorUtility.TryParseHtmlString(_playerData.baseColor, out baseColor));
        Debug.Log(ColorUtility.TryParseHtmlString(_playerData.linesColor, out linesColor));

        material.SetColor(BallCustomize._baseColorID, baseColor);
        material.SetColor(BallCustomize._linesColorID, linesColor);

        if(_playerData.usePattern)
        {
            material.SetTexture(BallCustomize._patternTextureID,
            BallCustomize._patternsStatic.First(x => x.name == _playerData.patternName));
        }
    }

    private void OnBallMoving(string playerSessionId, PlayerTransform playerTransform)
    {
        if(playerSessionId != PlayerID) return;
        DisablePhysic(playerTransform);
    }

    private void OnBallThrowing(string playerSessionId, Force throwForce)
    {
        if(playerSessionId != PlayerID) return;
        EnablePhysic(throwForce);
    }

    public void DisablePhysic(PlayerTransform ballTransform)
    {
        LastPos = new Vector3(ballTransform.X,
            ballTransform.Y,
            ballTransform.Z);

        LastRot = Quaternion.Euler(new Vector3(ballTransform.RotX,
            ballTransform.RotY,
            ballTransform.RotZ));

        if(Grabing == false)
        {
            Grabing = true;
            OnAir = false;

            InitGrabing();

            NetworkBallRB.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            NetworkBallRB.isKinematic = true;
            NetworkBallRB.detectCollisions = false;

            if(BallGrabing == null)
            {
                BallGrabing = StartCoroutine(BallMoving());
            }
            else
            {
                StopCoroutine(BallGrabing);
                BallGrabing = StartCoroutine(BallMoving());
            }
        }
    }

    public void EnablePhysic(Force throwForce)
    {
        NetworkBallTransform.position = new Vector3(throwForce.posX, throwForce.posY, throwForce.posZ);
        NetworkBallTransform.rotation = Quaternion.Euler(new Vector3(throwForce.rotX,
            throwForce.rotY,
            throwForce.rotZ));

        Vector3 force = new Vector3(throwForce.X, throwForce.Y, throwForce.Z);
        Vector3 torque = new Vector3(throwForce.torqueX, throwForce.torqueY, throwForce.torqueZ);

        if(C_ChekBallHigh == null)
        {
            C_ChekBallHigh = StartCoroutine(ChekBallHigh());
        }
        else
        {
            StopCoroutine(C_ChekBallHigh);
            C_ChekBallHigh = null;
        }

        StopCoroutine(BallGrabing);

        OnAir = true;
        
        if(BallOnAir == null)
        {
            BallOnAir = StartCoroutine(WaitingBallOnAir());
        }
        else
        {
            StopCoroutine(BallOnAir);
            BallOnAir = StartCoroutine(WaitingBallOnAir());
        }

        NetworkBallRB.isKinematic = false;
        NetworkBallRB.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        NetworkBallRB.detectCollisions = true;
        

        NetworkBallRB.AddForce(force);
        NetworkBallRB.AddTorque(torque);

        Grabing = false;
    }

    private void InitGrabing()
    {
        if(C_ChekBallHigh != null)
        {
            StopCoroutine(C_ChekBallHigh);
            C_ChekBallHigh = null;
        }

        BallOnParket = false;
        ParketHitCount = 0;
        BallCorrectHigh = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 0.1f)
        {
            _bouncesSource.clip = BouncesSound[Random.Range(0, BouncesSound.Count)];
            _bouncesSource.Play();
        }

        if(collision.transform.tag == "Parket")
        {
            BallOnParket = true;

            ParketHitCount++;

            if(BallOnAir != null)
            {
                StopCoroutine(BallOnAir);
                BallOnAir = null;
            }
            
            if(ParketHitCount == 1)
            {
                // if(GameRulesScript.isGame == true && OnAir == true)
                // {
                //     var HitPoint = collision.contacts[0];
                //     if(itsPoint == false) GameRulesScript.SetMarkerVisable(new Vector3(HitPoint.point.x, 0, HitPoint.point.z));
                // }
            }
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
                NetworkBallRB.AddForce(new Vector3(2,2,0), ForceMode.Impulse);
                break;
            }
        }
        yield break;
    }

    private IEnumerator BallMoving()
    {
        NetworkBallTransform.position = LastPos;
        NetworkBallTransform.rotation = LastRot;

        while(true)
        {
            NetworkBallTransform.position = Vector3.Lerp(NetworkBallTransform.position, LastPos, 0.5f);
            NetworkBallTransform.rotation = Quaternion.Lerp(NetworkBallTransform.rotation, LastRot, 0.5f);
            yield return null;
        }
    }

    public IEnumerator ChekBallHigh()
    {
        while(true)
        {
            yield return null;
            if(NetworkBallTransform.position.y >= 2.8f)
            {
                BallCorrectHigh = true;
                break;
            }
        }
        yield break;
    }
}
