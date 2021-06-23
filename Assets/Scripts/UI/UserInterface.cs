using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


//  данный скрипт описывает работу главного меню игры,
//  представляющего собой 3D-объекты
public class UserInterface : MonoBehaviour
{
    private DepthOfField _depthOfField;
    private ColorAdjustments _colorAdjustments;
    private ColorCurves _colorCurves;

    [SerializeField]
    public VolumeProfile VolumeProfile;
    [SerializeField]
    public GameObject BackgroundBall;
    [SerializeField]
    public GameObject BackgroundCameraTransform;
    [SerializeField]
    public GameObject MainMenuCamera;
    [SerializeField]
    public GameObject MainMenuTransform;
    [SerializeField]
    public GameObject MainMenuBackground;
    private GameObject _currentBackgroundMenuPrefab = null;

    private Vector3 _backgroundCameraStartPos;
    private Quaternion _backgroundCameraStartRot;

    private Vector3 _mainMenuCameraStartPos;
    private Quaternion _mainMenuCameraStartRot;

    private Vector3 _backgroundBallStartPos;
    private Quaternion _backgroundBallStartRot;

    private Coroutine C_dofChanging;
    private Coroutine C_SaturationChanging;
    private Coroutine C_BackgroundSceneAnimation;

    private Camera _cameraMainMenu;

    private List<GameObject> _uiButtonsList = new List<GameObject>();
    private List<GameObject> _uiSlidesList = new List<GameObject>();

    private AudioPlayerLogic _audioPlayer;

    private GUI _guiScript;
    private RoomList _roomListScript;

    [HideInInspector]
    public GameObject BallCustomizePanel;

    public bool AbleToMouseClick;
    public bool AbleToShowMenu;

    void Start()
    {
        BallCustomizePanel = GameObject.Find("BallCustomizePanel");
        BallCustomizePanel.SetActive(false);

        _guiScript = FindObjectOfType<GUI>();
        _audioPlayer = FindObjectOfType<AudioPlayerLogic>();
        _roomListScript = FindObjectOfType<RoomList>();

        AbleToMouseClick = true;

        _uiButtonsList = GameObject.FindGameObjectsWithTag("UI Button").ToList();
        _uiSlidesList = GameObject.FindGameObjectsWithTag("UI Slide").ToList();

        foreach(var Slide in _uiSlidesList)
        {
            var Buttons = Slide.GetComponentsInChildren<MenuButton>();
            foreach(var Button in Buttons)
            {
                Button.FadeIn();
            }
            Slide.SetActive(false);
        }

        _cameraMainMenu = MainMenuCamera.GetComponent<Camera>();

        VolumeProfile.TryGet(out _depthOfField);
        VolumeProfile.TryGet(out _colorAdjustments);
        VolumeProfile.TryGet(out _colorCurves);

        // SetSaturation(-100);
        // SetDoF(0);

        _backgroundCameraStartPos = BackgroundCameraTransform.transform.localPosition;
        _backgroundCameraStartRot = BackgroundCameraTransform.transform.localRotation;

        _mainMenuCameraStartPos = MainMenuCamera.transform.localPosition;
        _mainMenuCameraStartRot = MainMenuCamera.transform.localRotation;

        _backgroundBallStartPos = BackgroundBall.transform.localPosition;
        _backgroundBallStartRot = BackgroundBall.transform.localRotation;
    }

    public void OnGameMenuEnter()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;

        string SlideName;
        try
        {
            GUI.ShowGameUI(false);

            SlideName = "PausedSlide";
        }
        catch
        {
           SlideName = "FirstSlide";
        }

        var Slide = _uiSlidesList.Where(x => x.name == SlideName).ToList().Last();
        Slide.SetActive(true);
        foreach(var Button in Slide.GetComponentsInChildren<MenuButton>())
        {
            Button.FadeOut();
        }

        _currentBackgroundMenuPrefab = Instantiate(MainMenuBackground, new Vector3(0, 0, 0), Quaternion.Euler(new Vector3(90, 0, -90)), transform);
        _currentBackgroundMenuPrefab.transform.localPosition = new Vector3(-2, 0, 0);
        // AudioPlayer.BallVizualization(true);

        ChangeDepthOfField(0, 3);
        // ChangeSaturation(-100, 3);

        BackgroundSceneActivation(true);
        AbleToShowMenu = false;
    }

    public void OnGameMenuExit()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        GUI.ShowGameUI(true);

        try
        {
            var Slide = _uiSlidesList.Where(x => x.activeSelf).ToList().Last();
            foreach(var Button in Slide.GetComponentsInChildren<MenuButton>())
            {
                Button.FadeIn();
            }
        }
        catch
        {

        }

        Destroy(_currentBackgroundMenuPrefab);
        // AudioPlayer.BallVizualization(false);

        ChangeDepthOfField(300, 3);
        // ChangeSaturation(0, 3);

        BackgroundSceneActivation(false);
        AbleToShowMenu = true;
        
        GUI.ShowGameRoomsPanel(false);
    }

    public void OnUIButtonPressed(string ButtonName)
    {
        StartCoroutine(OnButtonPressedHandler(ButtonName));
    }

    public void OnSlideChanged(string oldSlideName, string newSlideName)
    {
        var oldSlide = _uiSlidesList.Find(x => x.name == oldSlideName);
        var newSlide = _uiSlidesList.Find(x => x.name == newSlideName);

        oldSlide.SetActive(false);
        newSlide.SetActive(true);

        var Buttons = newSlide.GetComponentsInChildren<MenuButton>();

        foreach(var Button in Buttons)
        {
            Button.FadeOut();
        }
    }

    private void BackgroundSceneActivation(bool isStart)
    {
        InitElementsTransform();

        if(isStart)
        {
            if(GameManager.PlayerController != null && GameManager.PlayerController.activeSelf)
            {
                FindObjectOfType<PlayerController>().MovementInit(false);
                GameManager.PlayerController.SetActive(false);
            }

            if(C_BackgroundSceneAnimation != null) StopCoroutine(C_BackgroundSceneAnimation);
            C_BackgroundSceneAnimation = StartCoroutine(BackgroundSceneAnimation());

            BackgroundCameraTransform.SetActive(true);
            MainMenuCamera.SetActive(true);
        }
        else
        {
            StopCoroutine(C_BackgroundSceneAnimation);
            C_BackgroundSceneAnimation = null;

            BackgroundCameraTransform.SetActive(false);
            MainMenuCamera.SetActive(false);

            if(!GameManager.PlayerController.activeSelf)
            {
                GameManager.PlayerController.SetActive(true);
                FindObjectOfType<PlayerController>().MovementInit(true);
            }
        }
    }





    public void ChangeDepthOfField(float Value, float Time)
    {
        if(C_dofChanging != null)
        {
            StopCoroutine(C_dofChanging);
            C_dofChanging = null;
        }

        C_dofChanging = StartCoroutine(DepthOfFieldChanging(Value, Time));
    }

    public void ChangeSaturation(float Value, float Time)
    {
        if(C_SaturationChanging != null)
        {
            StopCoroutine(C_SaturationChanging);
            C_SaturationChanging = null;
        }

        C_SaturationChanging = StartCoroutine(SaturationChanging(Value, Time));
    }

    private IEnumerator DepthOfFieldChanging(float Value, float LerpTime)
    {
        float LerpProc = 0;
        float BeginValue = _depthOfField.gaussianEnd.value;
        yield return null;

        while(LerpProc < 1)
        {
            _depthOfField.gaussianEnd.value = Mathf.Lerp(BeginValue, Value, LerpProc);
            LerpProc += LerpTime * Time.deltaTime;

            yield return null;
        }

        _depthOfField.gaussianEnd.value = Value;
    }

    private IEnumerator SaturationChanging(float Value, float LerpTime)
    {
        float LerpProc = 0;
        float BeginValue = _colorAdjustments.saturation.value;
        yield return null;

        while(LerpProc < 1)
        {
            _colorAdjustments.saturation.value = Mathf.Lerp(BeginValue, Value, LerpProc);
            LerpProc += LerpTime * Time.deltaTime;

            yield return null;
        }

        _colorAdjustments.saturation.value = Value;
    }

    private IEnumerator BackgroundSceneAnimation()
    {
        float RotationSmooth = 0.005f;
        Vector3 MousePosition = new Vector3();
        float MouseX = 0;
        float MouseY = 0;

        float CameraSpeed = 5f;

        yield return null;

        while(true)
        {
            MousePosition = new Vector3(Mathf.Clamp(Input.mousePosition.x, 0, Screen.width), Mathf.Clamp(Input.mousePosition.y, 0, Screen.height), 0);

            MouseX = MousePosition.x - (Screen.width / 2); 
            MouseY = MousePosition.y - (Screen.height / 2);

            MouseX *= RotationSmooth;
            MouseY *= RotationSmooth;

            BackgroundBall.transform.rotation = Quaternion.Euler(new Vector3(0, -MouseX, MouseY));
            BackgroundCameraTransform.transform.localRotation = Quaternion.Euler(new Vector3(MouseY * CameraSpeed, -(MouseX * CameraSpeed) - 90, 0));

            MainMenuTransform.transform.localRotation = Quaternion.Euler(new Vector3(0, -MouseX, MouseY));

            yield return null;
        }
    }



    public void CreateGameModeRoom(string modeName)
    {
        FindObjectOfType<Network>().SendServerData("ServerOnNewRoomRequest", false, Network.accountData.login, modeName);
        OnGameMenuExit();
    }

    public void SetSaturation(float Value)
    {
        _colorAdjustments.saturation.value = Value;
    }

    public void SetDoF(float Value)
    {
        _depthOfField.gaussianEnd.value = Value;
    }

    private void InitElementsTransform()
    {
        BackgroundCameraTransform.transform.localPosition = _backgroundCameraStartPos;
        BackgroundCameraTransform.transform.localRotation = _backgroundCameraStartRot;

        MainMenuCamera.transform.localPosition = _mainMenuCameraStartPos;
        MainMenuCamera.transform.localRotation = _mainMenuCameraStartRot;

        BackgroundBall.transform.localPosition = _backgroundBallStartPos;
        BackgroundBall.transform.localRotation = _backgroundBallStartRot;
    }

    private IEnumerator OnButtonPressedHandler(string ButtonName)
    {
        string SlideName = GameObject.Find(ButtonName).transform.parent.name;
        AbleToMouseClick = false;

        var ButtonsInCurrentSlide = _uiSlidesList.Find(x => x.name == SlideName).GetComponentsInChildren<MenuButton>();

        foreach(var UIButton in ButtonsInCurrentSlide)
        {
            UIButton.FadeIn();
        }

        yield return new WaitForSeconds(0.4f);

        GameObject.Find(SlideName).SetActive(false);

        AbleToMouseClick = true;

        switch(ButtonName)
        {
            case "Text_SignIn":
                GUI.ShowRegistrationPanel(true);
                FindObjectOfType<RegPanel>().SetFadeEffect("signin");
                break;
            case "Text_Quit":
                yield return new WaitForSeconds(0.5f);
                Application.Quit();
                break;
            case "Text_ResumeGame":
                OnGameMenuExit();
                break;
            case "Text_GameModes":
                OnSlideChanged(SlideName, "GameModesSlide");
                break;
            case "Text_Multiplayer":
                GUI.ShowGameRoomsPanel(true);
                break;
        }

        yield break;
    }
}
