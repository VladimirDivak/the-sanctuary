using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CameraRaycast : MonoBehaviour
{
    public static Transform CameraTransform;

    private bool AbleToClick = true;
    private CharacterController Player;
    private Camera currentCamera;

    public static Vector3 BallForce;
    public static float DistanceToPoint;

    public static bool IsBoard1;
    public static bool IsBoard2;

    private BallLogic BallScript;
    private PlayerController PlayerControllerScript;
    private AudioPlayerLogic AudioScript;
    public static string CurrentBall;

    public Transform ShootPoint;
    public float Y = 3.82f;
    public float coef = 0.42f;

    public bool AbleToShowPointCam;

    private bool BoardDetected;
    // private GamemodeSelector GamemodeScript;

    void Start()
    {
        currentCamera = Camera.main;
        // GamemodeScript = FindObjectOfType<GamemodeSelector>();
        BallScript = GameObject.FindObjectOfType<BallLogic>();
        AudioScript = FindObjectOfType<AudioPlayerLogic>();
        ShootPoint = GameObject.Find("ShootPoint").transform;
        //GameRulesScript = GameObject.Find("CANDYSHOP").GetComponent<NetworkGameRules>();
        CameraTransform = transform;
    }

    public void OnGameInit()
    {
        PlayerControllerScript = GameObject.Find("PlayerController").GetComponent<PlayerController>();
    }

    void Update()
    {
        if(GameManager.SetControl == true) Raycast();
    }

    private void Raycast()
    {
        Ray CameraRay = currentCamera.ScreenPointToRay(new Vector3(Screen.width/2, Screen.height/2, CameraTransform.position.z));

        RaycastHit CameraHit;
        RaycastHit CameraToRingHit;

        if(Input.GetMouseButton(0))
        {
            if(Physics.Raycast(CameraRay, out CameraHit, Mathf.Infinity) && !BallScript.IsGrabed)
            {
                if(CameraHit.transform.tag == "Ball")
                {
                    BallScript = CameraHit.transform.GetComponent<BallLogic>();

                    if (!BallScript.OnAir)
                    {
                        BallScript.PhysicDisable();
                    }
                }
                
                if(CameraHit.transform.name == "LED_next" && AbleToClick)
                {

                    StartCoroutine(StartTimerForAbleToClick());
                    AudioScript.OnNextTrack();
                }
                else if(CameraHit.transform.name == "LED_preview" && AbleToClick)
                {
                    StartCoroutine(StartTimerForAbleToClick());
                    AudioScript.OnPreviewTrack();
                }
            }

            if(BallScript.IsGrabed)
            {
                if(Physics.Raycast(CameraRay, out CameraToRingHit, Mathf.Infinity, 9))
                {
                    if(CameraHit.transform.name == "Board1Trigger")
                    {
                        IsBoard1 = true;
                        AbleToShowPointCam = true;

                        if(BoardDetected == false)
                        {
                            OnBoardDetected(true);
                        }
                    }
                    else if(CameraHit.transform.name == "Board2Trigger")
                    {
                        IsBoard2 = true;
                        AbleToShowPointCam = true;

                        if(BoardDetected == false)
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
                    coef = 0.42f * 4.6f / BallScript.Maginude;
                    BallScript.ForceConst = Mathf.Clamp(0.0088445f * Mathf.Pow(BallScript.Maginude, 4) -0.2743184f * Mathf.Pow(BallScript.Maginude, 3) + 2.8879387f * Mathf.Pow(BallScript.Maginude, 2) - 5.0169378f * BallScript.Maginude + 82.8919099f, 0, 160);

                    Vector3 ShootPos = CameraTransform.position - (CameraTransform.rotation * new Vector3(0, 0, -BallScript.Maginude * coef));

                    ShootPoint.position = new Vector3(Mathf.Clamp(ShootPos.x, -17, 17), Y, Mathf.Clamp(ShootPos.z, -17, 17));
                }
                else
                {
                    ShootPoint.position = CameraTransform.position + CameraTransform.forward * 6;
                    BallScript.ForceConst = 100;
                }
            }
        }

        if(Input.GetMouseButtonUp(0) && BallScript.IsGrabed)
        {
            BallScript.PhysicEnable();
        }
    }

    private IEnumerator StartTimerForAbleToClick()
    {
        AbleToClick = false;
        yield return new WaitForSeconds(0.5f);
        AbleToClick = true;
        yield break;
    }

    private void OnBoardDetected(bool _onDetected)
    {
        if(_onDetected == true)
        {
            if(BoardDetected == false)
            {
                BoardDetected = true;
                BallScript.SetForceOffset(true);
            }
        }
        else
        {
            if(BoardDetected == true)
            {
                BoardDetected = false;
                BallScript.SetForceOffset(false);
            }
        }
    }
}