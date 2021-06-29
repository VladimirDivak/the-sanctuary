using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheSanctuary;
using System.Linq;

//  данный класс описывает поведение баскетбольного мяча других игроков в игре
//
//  в идеале стоит не делить логику поведения мяча клиента и мячей
//  сетевых игроков, а создать общий интерфейс, чтобы при попадании
//  мяча в кольцо триггер не пытался получить компонент скрипта того
//  или иного типа мяча
//
//  данный скрипт был описан мной практически в начале работы над этим
//  проектом, по этому здесь имеется несколько спорных решений, которые
//  в дальнейшем я намерен изменить

public class NetworkBall : MonoBehaviour
{
    [SerializeField]
    public List<AudioClip> BouncesSound = new List<AudioClip>();

    public string PlayerID;
    private Player _playerData;

    public string PlayerName => _playerData.login;

    private AudioSource _bouncesSource;

    public Transform NetworkBallTransform;
    public Rigidbody NetworkBallRB;

    public int ParketHitCount = 0;
    public bool OnAir;
    public bool itsPoint;

    private bool _grabing;

    private Vector3 _lastPos;
    private Quaternion _lastRot;

    private Coroutine C_BallOnAir;
    private Coroutine C_BallGrabing;
    public Coroutine C_ChekBallHigh;

    private bool _ballOnParket;
    public bool BallCorrectHigh;

    private GUI _gui;
    private Camera _mainCamera;

    private void Start()
    {
        _gui = FindObjectOfType<GUI>();
        _mainCamera = Camera.main;

        NetworkBallTransform = transform;
        _bouncesSource = GetComponent<AudioSource>();
        NetworkBallRB = GetComponent<Rigidbody>();

        Network.OnBallMovingEvent += OnBallMoving;
        Network.OnBallThrowingEvent += OnBallThrowing;

        FindObjectOfType<GameManager>().ClothColliders.Add(new ClothSphereColliderPair(GetComponent<SphereCollider>(), GetComponent<SphereCollider>()));
    }

    private void Update()
    {
        if(_mainCamera.isActiveAndEnabled)
        {
            var nicknamePosition = new Vector3(NetworkBallTransform.position.x,
                NetworkBallTransform.position.y + 0.3f,
                NetworkBallTransform.position.z);

            _gui.SetPlayerNicknamePosition(_playerData.login,
                _mainCamera.WorldToScreenPoint(nicknamePosition));
            Debug.Log(_mainCamera.transform.TransformPoint(NetworkBallTransform.position - _mainCamera.transform.position));
        }
    }

    //  метод вызывается в ObjectSpawner'е и инициализирует полученные с сервера
    //  данные: ID текущей сессии, персональные данные клиента, его статус готовности к игре
    public void SetNetworkBallData(string id, PersonalData data, bool readyStatus)
    {
        PlayerID = id;
        transform.name = PlayerID;

        _playerData = new Player(data);

        Color baseColor;
        Color linesColor;

        Material material = GetComponent<Renderer>().sharedMaterial;

        ColorUtility.TryParseHtmlString(_playerData.BallOutlook.BaseColor, out baseColor);
        ColorUtility.TryParseHtmlString(_playerData.BallOutlook.LinesColor, out linesColor);

        material.SetColor(BallCustomize.baseColorID, baseColor);
        material.SetColor(BallCustomize.linesColorID, linesColor);

        if(_playerData.BallOutlook.UsePattern)
        {
            var patternTexture = FindObjectOfType<ObjectSpawner>().Patterns.First(x => x.name.Contains(_playerData.BallOutlook.PatternName));
            material.SetTexture(BallCustomize.patternTextureID, patternTexture);
            material.SetInt(BallCustomize.usePatternID, 1);
        }
        else material.SetInt(BallCustomize.usePatternID, 0);

        material.SetInt(BallCustomize.useNetworkFresnelID, 1);
    }

    //  метод вызывается в Network и запускает цепочку методов, которые описывают
    //  логику перемещения мяча
    private void OnBallMoving(string playerSessionId, PlayerTransform playerTransform)
    {
        if(playerSessionId != PlayerID) return;
        DisablePhysic(playerTransform);
    }

    //  метод вызывается в Network, запускает цепочку методов при броске игроком мяча
    private void OnBallThrowing(string playerSessionId, Force throwForce)
    {
        if(playerSessionId != PlayerID) return;
        EnablePhysic(throwForce);
    }

    public void DisablePhysic(PlayerTransform ballTransform)
    {
        _lastPos = new Vector3(ballTransform.X,
            ballTransform.Y,
            ballTransform.Z);

        _lastRot = Quaternion.Euler(new Vector3(ballTransform.RotX,
            ballTransform.RotY,
            ballTransform.RotZ));

        if(_grabing == false)
        {
            _grabing = true;
            OnAir = false;

            InitGrabing();

            NetworkBallRB.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            NetworkBallRB.isKinematic = true;
            NetworkBallRB.detectCollisions = false;

            if(C_BallGrabing != null)
            {
                StopCoroutine(C_BallGrabing);
                C_BallGrabing = null;

            }
            C_BallGrabing = StartCoroutine(BallMoving());
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

        StopCoroutine(C_BallGrabing);

        OnAir = true;
        
        if(C_BallOnAir == null)
        {
            C_BallOnAir = StartCoroutine(WaitingBallOnAir());
        }
        else
        {
            StopCoroutine(C_BallOnAir);
            C_BallOnAir = StartCoroutine(WaitingBallOnAir());
        }

        NetworkBallRB.isKinematic = false;
        NetworkBallRB.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        NetworkBallRB.detectCollisions = true;
        

        NetworkBallRB.AddForce(force);
        NetworkBallRB.AddTorque(torque);

        _grabing = false;
    }

    private void InitGrabing()
    {
        if(C_ChekBallHigh != null)
        {
            StopCoroutine(C_ChekBallHigh);
            C_ChekBallHigh = null;
        }

        _ballOnParket = false;
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
            _ballOnParket = true;

            ParketHitCount++;

            if(C_BallOnAir != null)
            {
                StopCoroutine(C_BallOnAir);
                C_BallOnAir = null;
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

        while(_ballOnParket == false)
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
        NetworkBallTransform.position = _lastPos;
        NetworkBallTransform.rotation = _lastRot;

        while(true)
        {
            NetworkBallTransform.position = Vector3.Lerp(NetworkBallTransform.position, _lastPos, 0.5f);
            NetworkBallTransform.rotation = Quaternion.Lerp(NetworkBallTransform.rotation, _lastRot, 0.5f);
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
