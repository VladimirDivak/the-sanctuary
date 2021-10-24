using System.Collections;
using UnityEngine;

//  данный класс представляет собой описание логики поведения камеры игрока
//  класс был создан в начале работы над проектом и с тех пор мало подвергался
//  каким-либо изменениям

public class CameraRaycast : MonoBehaviour
{
    public static CameraRaycast Instance { get; private set; }

    public static Transform cameraTransform;

    private bool _ableToClick = true;
    private Camera _currentCamera;

    public static Vector3 ballForce;
    public static float distanceToPoint;

    public static bool isBoard1;
    public static bool isBoard2;

    private PlayerBall _ballScript;
    private PlayerController _playerControllerScript;
    private AudioPlayer _audioScript;

    public Transform shootPoint;
    public float Y = 3.82f;
    public float coef = 0.42f;

    public bool ableToShowPointCam;

    private bool _boardDetected;

    public void SetCurrentPlayerBall(PlayerBall ball) => _ballScript = ball;

    void Start()
    {
        Instance = this;

        _currentCamera = Camera.main;
        _audioScript = FindObjectOfType<AudioPlayer>();
        shootPoint = GameObject.Find("ShootPoint").transform;
        cameraTransform = transform;
    }

    public void OnGameInit()
    {
        _playerControllerScript = GameObject.Find("PlayerController").GetComponent<PlayerController>();
    }

    void Update()
    {
        if(GameManager.Instance.setControl == true) Raycast();
    }

    private void Raycast()
    {
        Ray cameraRay = _currentCamera.ScreenPointToRay(new Vector3(Screen.width/2, Screen.height/2, cameraTransform.position.z));

        if(Input.GetMouseButton(0))
        {
            if(_ballScript == null)
            {
                if(Physics.Raycast(cameraRay, out var cameraHit, Mathf.Infinity))
                {
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

                    if(cameraHit.transform.TryGetComponent<PlayerBall>(out _ballScript))
                    {
                        if (!_ballScript.onAir) _ballScript.PhysicDisable();
                    }
                }
            }
            else if(_ballScript.IsGrabed)
            {
                if(Physics.Raycast(cameraRay, out var cameraToRingHit, Mathf.Infinity, 9))
                {
                    if(cameraToRingHit.transform.name == "Board1Trigger")
                    {
                        isBoard1 = true;
                        ableToShowPointCam = true;

                        if(_boardDetected == false) OnBoardDetected(true);
                    }
                    else if(cameraToRingHit.transform.name == "Board2Trigger")
                    {
                        isBoard2 = true;
                        ableToShowPointCam = true;

                        if(_boardDetected == false) OnBoardDetected(true);
                    }
                    else
                    {
                        isBoard1 = false;
                        isBoard2 = false;
                        ableToShowPointCam = false;

                        OnBoardDetected(false);
                    }
                }

                if(isBoard1 || isBoard2)
                {
                    coef = 0.42f * 4.6f / _ballScript.magnitude;
                    _ballScript.ForceConst = Mathf.Clamp(0.0088445f * Mathf.Pow(_ballScript.magnitude, 4) -0.2743184f * Mathf.Pow(_ballScript.magnitude, 3) + 2.8879387f * Mathf.Pow(_ballScript.magnitude, 2) - 5.0169378f * _ballScript.magnitude + 82.8919099f, 0, 160);

                    Vector3 ShootPos = cameraTransform.position - (cameraTransform.rotation * new Vector3(0, 0, -_ballScript.magnitude * coef));

                    shootPoint.position = new Vector3(Mathf.Clamp(ShootPos.x, -17, 17), Y, Mathf.Clamp(ShootPos.z, -17, 17));
                }
                else
                {
                    shootPoint.position = cameraTransform.position + cameraTransform.forward * 6;
                    _ballScript.ForceConst = 100;
                }
            }
        }
        
        if(Input.GetMouseButtonUp(0) && _ballScript != null)
        {
            _ballScript.PhysicEnable();
            _ballScript = null;
        }
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
