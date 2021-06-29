using System.Collections;
using UnityEngine;

//  данный класс представляет собой описание логики поведения камеры игрока
//  класс был создан в начале работы над проектом и с тех пор мало подвергался
//  каким-либо изменениям

public class CameraRaycast : MonoBehaviour
{
    public static Transform CameraTransform;

    private bool _ableToClick = true;
    private Camera _currentCamera;

    public static Vector3 BallForce;
    public static float DistanceToPoint;

    public static bool IsBoard1;
    public static bool IsBoard2;

    private BallLogic _ballScript;
    private PlayerController _playerControllerScript;
    private AudioPlayer _audioScript;
    public static string CurrentBall;

    public Transform ShootPoint;
    public float Y = 3.82f;
    public float coef = 0.42f;

    public bool AbleToShowPointCam;

    private bool _boardDetected;

    void Start()
    {
        _currentCamera = Camera.main;
        _ballScript = GameObject.FindObjectOfType<BallLogic>();
        _audioScript = FindObjectOfType<AudioPlayer>();
        ShootPoint = GameObject.Find("ShootPoint").transform;
        CameraTransform = transform;
    }

    public void OnGameInit()
    {
        _playerControllerScript = GameObject.Find("PlayerController").GetComponent<PlayerController>();
    }

    void Update()
    {
        if(GameManager.SetControl == true) Raycast();
    }

    private void Raycast()
    {
        Ray cameraRay = _currentCamera.ScreenPointToRay(new Vector3(Screen.width/2, Screen.height/2, CameraTransform.position.z));

        RaycastHit cameraHit;
        RaycastHit cameraToRingHit;

        //  так как на сегодняшний момент игра не подразумевает оповощения игрока о том, что
        //  он навёл камеру на тот или иной предмет, логика рейкастов запускается по нажатию
        //  левой кнопкой мыши (или при удерживании пальца на экране телефона)
        if(Input.GetMouseButton(0))
        {
            if(Physics.Raycast(cameraRay, out cameraHit, Mathf.Infinity) && !_ballScript.IsGrabed)
            {
                if(cameraHit.transform.tag == "Ball")
                {
                    _ballScript = cameraHit.transform.GetComponent<BallLogic>();

                    if (!_ballScript.OnAir)
                    {
                        _ballScript.PhysicDisable();
                    }
                }
                
                if(cameraHit.transform.name == "LED_next" && _ableToClick)
                {

                    StartCoroutine(StartTimerForAbleToClick());
                    _audioScript.OnNextTrack();
                }
                else if(cameraHit.transform.name == "LED_preview" && _ableToClick)
                {
                    StartCoroutine(StartTimerForAbleToClick());
                    _audioScript.OnPreviewTrack();
                }
            }

            if(_ballScript.IsGrabed)
            {
                if(Physics.Raycast(cameraRay, out cameraToRingHit, Mathf.Infinity, 9))
                {
                    if(cameraHit.transform.name == "Board1Trigger")
                    {
                        IsBoard1 = true;
                        AbleToShowPointCam = true;

                        if(_boardDetected == false)
                        {
                            OnBoardDetected(true);
                        }
                    }
                    else if(cameraHit.transform.name == "Board2Trigger")
                    {
                        IsBoard2 = true;
                        AbleToShowPointCam = true;

                        if(_boardDetected == false)
                        {
                            OnBoardDetected(true);
                        }
                    }
                    else
                    {
                        IsBoard1 = false;
                        IsBoard2 = false;
                        AbleToShowPointCam = false;

                        OnBoardDetected(false);
                    }
                }

                if(IsBoard1 || IsBoard2)
                {
                    //  на тот момент это было лучшим решением по заданию траектории полёта мяча в сторону корзины
                    //  
                    //  формула была выведена посредством нахождения 10ти точек броска мяча в кольцо вдоль одной прямой,
                    //  после чего значения были подставлены в определенный вид математического уравнения, чтобы высчитать
                    //  промежуточные результаты
                    //  
                    //  после этого мяч стал с вероятностью в 98% попадать в кольцо, из-за чего пришлось вводить различные
                    //  коэффициенты для внесения погрешности
                    coef = 0.42f * 4.6f / _ballScript.Maginude;
                    _ballScript.ForceConst = Mathf.Clamp(0.0088445f * Mathf.Pow(_ballScript.Maginude, 4) -0.2743184f * Mathf.Pow(_ballScript.Maginude, 3) + 2.8879387f * Mathf.Pow(_ballScript.Maginude, 2) - 5.0169378f * _ballScript.Maginude + 82.8919099f, 0, 160);

                    Vector3 ShootPos = CameraTransform.position - (CameraTransform.rotation * new Vector3(0, 0, -_ballScript.Maginude * coef));

                    ShootPoint.position = new Vector3(Mathf.Clamp(ShootPos.x, -17, 17), Y, Mathf.Clamp(ShootPos.z, -17, 17));
                }
                else
                {
                    ShootPoint.position = CameraTransform.position + CameraTransform.forward * 6;
                    _ballScript.ForceConst = 100;
                }
            }
        }

        if(Input.GetMouseButtonUp(0) && _ballScript.IsGrabed)
            _ballScript.PhysicEnable();
    }

    private IEnumerator StartTimerForAbleToClick()
    {
        _ableToClick = false;
        yield return new WaitForSeconds(0.5f);
        _ableToClick = true;
        yield break;
    }

    private void OnBoardDetected(bool _onDetected)
    {
        if(_onDetected == true)
        {
            if(_boardDetected == false)
            {
                _boardDetected = true;
                _ballScript.SetForceOffset(true);
            }
        }
        else
        {
            if(_boardDetected == true)
            {
                _boardDetected = false;
                _ballScript.SetForceOffset(false);
            }
        }
    }
}
