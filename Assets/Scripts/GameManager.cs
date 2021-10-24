using System.Collections.Generic;
using UnityEngine;

//  класс с некрасивым названием работает с общей
//  логикой поведения игры

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [HideInInspector]
    public GameObject cameraMain { get; private set; }
    [HideInInspector]
    public PlayerController playerController { get; private set; }

    private GUI _guiScript;

    [HideInInspector]
    public bool setControl = true;
    [HideInInspector]
    public bool setMovingControl = true;
    [HideInInspector]
    public bool gameStarted;

    private Vector3 _startCameraPosition;
    private Quaternion _startCameraRotation;

    private Cloth[] _netsClothes;

    public List<ClothSphereColliderPair> ClothColliders = new List<ClothSphereColliderPair>();
    private BackgroundCameraAnimation _bgCamera;

    private UserInterface _ui;

    void Start()
    {
        Instance = this;

        _guiScript = FindObjectOfType<GUI>();

        // _ui = GameObject.FindObjectOfType<UserInterface>();
        // _ui.OnGameMenuEnter();

        _netsClothes = GameObject.FindObjectsOfType<Cloth>();

        cameraMain = Camera.main.gameObject;
        
        _startCameraPosition = cameraMain.transform.position;
        _startCameraRotation = cameraMain.transform.rotation;

        playerController = FindObjectOfType<PlayerController>();
        playerController.gameObject.SetActive(false);

        // _bgCamera = GameObject.FindObjectOfType<BackgroundCameraAnimation>();
        // _bgCamera.StartBackgroundCameraMoving();

        InitializationGame();
    }

    public void InitializationGame()
    {
        playerController.gameObject.SetActive(true);

        // Destroy(GameObject.Find("Background"));
        // Destroy(GameObject.Find("LearningPanel"));
        // Destroy(GameObject.Find("LoginPanel"));
        // Destroy(GameObject.Find("BallCustomizePanel"));

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // _ui.OnGameMenuExit();
        // GUI.ShowRegistrationPanel(false);
        // GUI.ShowGameUI(true);

        GameObject.FindObjectOfType<CameraRaycast>().OnGameInit();

        FindObjectOfType<ObjectSpawner>().SpawnBall(Network.accountData, new Vector3(12.369f, 1.23f, -1.345f));

        var Balls = GameObject.FindGameObjectsWithTag("Ball");
        foreach(var Ball in Balls)
        {
            ClothColliders.Add(new ClothSphereColliderPair(Ball.GetComponent<SphereCollider>(), Ball.GetComponent<SphereCollider>()));
        }
        

        foreach(var cloth in _netsClothes)
        {
            cloth.sphereColliders = ClothColliders.ToArray();
        }

        gameStarted = true;

        // GUI.OnFadeOut -= InitializationGame;
    }

    void Update()
    {
        // if(Input.GetKeyDown(KeyCode.Escape) && gameStarted)
        // {
        //     if(_ui.AbleToShowMenu) _ui.OnGameMenuEnter();
        //     else _ui.OnGameMenuExit();
        // }
    }
}
