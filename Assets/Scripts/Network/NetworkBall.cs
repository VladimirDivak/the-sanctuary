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

public class NetworkBall : Ball
{
    public string PlayerID;
    private Player _playerData;

    public string PlayerName => _playerData.login;

    private Coroutine c_ballGrabing;


    // private GUI _gui;
    // private Camera _mainCamera;

    private void Start()
    {
        _transform = transform;
        _bouncesSource = GetComponent<AudioSource>();
        _rigidBody = GetComponent<Rigidbody>();

        Network.OnBallMovingEvent += OnBallMoving;
        Network.OnBallThrowingEvent += OnBallThrowing;

        FindObjectOfType<GameManager>().ClothColliders.Add(new ClothSphereColliderPair(GetComponent<SphereCollider>(), GetComponent<SphereCollider>()));
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
        _lastPosition = new Vector3(ballTransform.X,
            ballTransform.Y,
            ballTransform.Z);

        _lastRotation = Quaternion.Euler(new Vector3(ballTransform.RotX,
            ballTransform.RotY,
            ballTransform.RotZ));

        if(isGrabed == false)
        {
            isGrabed = true;
            onAir = false;

            InitGrabing();

            _rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            _rigidBody.isKinematic = true;
            _rigidBody.detectCollisions = false;

            if(c_ballGrabing != null)
            {
                StopCoroutine(c_ballGrabing);
                c_ballGrabing = null;

            }
            c_ballGrabing = StartCoroutine(BallMoving());
        }
    }

    public void EnablePhysic(Force throwForce)
    {
        _transform.position = new Vector3(throwForce.posX, throwForce.posY, throwForce.posZ);
        _transform.rotation = Quaternion.Euler(new Vector3(throwForce.rotX,
            throwForce.rotY,
            throwForce.rotZ));

        Vector3 force = new Vector3(throwForce.X, throwForce.Y, throwForce.Z);
        Vector3 torque = new Vector3(throwForce.torqueX, throwForce.torqueY, throwForce.torqueZ);

        if(c_chekBallHigh != null) StopCheckBallHight();
        c_chekBallHigh = StartCoroutine(ChekHigh());

        StopCoroutine(c_ballGrabing);

        onAir = true;
        
        if(c_ballOnAir == null)
        {
            c_ballOnAir = StartCoroutine(WaitingBallOnAir());
        }
        else
        {
            StopCoroutine(c_ballOnAir);
            c_ballOnAir = StartCoroutine(WaitingBallOnAir());
        }

        _rigidBody.isKinematic = false;
        _rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        _rigidBody.detectCollisions = true;
        

        _rigidBody.AddForce(force);
        _rigidBody.AddTorque(torque);

        isGrabed = false;
    }

    private void InitGrabing()
    {
        StopCheckBallHight();

        _ballOnParket = false;
        ParketHitCount = 0;
        ballCorrectHigh = false;
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        if(collision.transform.tag == "Parket")
        {
            _ballOnParket = true;

            ParketHitCount++;

            if(c_ballOnAir != null)
            {
                StopCoroutine(c_ballOnAir);
                c_ballOnAir = null;
            }
        }
    }

    private IEnumerator BallMoving()
    {
        _transform.position = _lastPosition;
        _transform.rotation = _lastRotation;

        while(true)
        {
            _transform.position = Vector3.Lerp(_transform.position, _lastPosition, 0.5f);
            _transform.rotation = Quaternion.Lerp(_transform.rotation, _lastRotation, 0.5f);
            yield return null;
        }
    }
}
