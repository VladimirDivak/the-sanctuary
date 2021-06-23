using System.Collections.Generic;
using UnityEngine;

//  класс с некрасивым названием работает с общей
//  логикой поведения игры

public class GameManager : MonoBehaviour
{
    public static GameObject MainCamera;
    public static GameObject PlayerController;
    public static GameObject ShootPoint;

    private GUI _guiScript;

    public static bool SetControl = true;
    public static bool SetMovingControl = true;
    public static bool GameStarted;

    private Vector3 _startCameraPosition;
    private Quaternion _startCameraRotation;

    private Cloth[] _netsClothes;

    public List<ClothSphereColliderPair> ClothColliders = new List<ClothSphereColliderPair>();
    private BackgroundCameraAnimation _bgCamera;

    private UserInterface _ui;

    void Start()
    {
        _guiScript = FindObjectOfType<GUI>();

        _ui = GameObject.FindObjectOfType<UserInterface>();
        _ui.OnGameMenuEnter();

        _netsClothes = GameObject.FindObjectsOfType<Cloth>();

        MainCamera = GameObject.Find("PlayerController/Main Camera");
        _startCameraPosition = MainCamera.transform.position;
        _startCameraRotation = MainCamera.transform.rotation;

        PlayerController = GameObject.Find("PlayerController");
        PlayerController.SetActive(false);

        _bgCamera = GameObject.FindObjectOfType<BackgroundCameraAnimation>();
        _bgCamera.StartBackgroundCameraMoving();

        GUI.OnFadeOut += InitializationGame;
    }

    public void InitializationGame()
    {
        Destroy(GameObject.Find("Background"));
        Destroy(GameObject.Find("LearningPanel"));
        Destroy(GameObject.Find("LoginPanel"));
        Destroy(GameObject.Find("BallCustomizePanel"));

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _ui.OnGameMenuExit();
        GUI.ShowRegistrationPanel(false);
        GUI.ShowGameUI(true);

        GameObject.FindObjectOfType<CameraRaycast>().OnGameInit();

        var Balls = GameObject.FindGameObjectsWithTag("Ball");
        foreach(var Ball in Balls)
        {
            ClothColliders.Add(new ClothSphereColliderPair(Ball.GetComponent<SphereCollider>(), Ball.GetComponent<SphereCollider>()));
        }
        

        foreach(var cloth in _netsClothes)
        {
            cloth.sphereColliders = ClothColliders.ToArray();
        }

        GameStarted = true;
        GameObject.FindObjectOfType<ArtistNamePanel>().ShowNewArtistName();

        GUI.OnFadeOut -= InitializationGame;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && GameStarted)
        {
            if(_ui.AbleToShowMenu) _ui.OnGameMenuEnter();
            else _ui.OnGameMenuExit();
        }
    }
}
