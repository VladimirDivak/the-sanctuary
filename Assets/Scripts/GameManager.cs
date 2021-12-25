using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

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

    private Scene _simulationScene;
    private PhysicsScene _physicsScene;
    Vector3 _lastForce;

    [SerializeField]
    GameObject simulationBall;
    [SerializeField]
    GameObject[] transformsForPhysicsSimulation;

    GameObject _currentSimulationBall;

    public void CreatePhysicsSimulationScene()
    {
        _simulationScene = SceneManager.CreateScene("Simulation", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        _physicsScene = _simulationScene.GetPhysicsScene();

        foreach(var transformData in transformsForPhysicsSimulation)
        {
            var obj = Instantiate(transformData.gameObject, transformData.transform.position, transformData.transform.rotation);
            if(obj.TryGetComponent<MeshRenderer>(out var meshRendererData)) meshRendererData.enabled = false;

            SceneManager.MoveGameObjectToScene(obj, _simulationScene);
        }

        _currentSimulationBall = Instantiate(simulationBall, Vector3.zero, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(_currentSimulationBall, _simulationScene);
    }

    public Vector3[] SimulateTrajectory(Transform ballTransform, Vector3 force, ForceMode forceMode)
    {
        List<Vector3> points = new List<Vector3>();
        var rb = _currentSimulationBall.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        var collider = _currentSimulationBall.GetComponent<SphereCollider>();

        _currentSimulationBall.transform.position = ballTransform.position;
        _currentSimulationBall.transform.rotation = ballTransform.rotation;

        rb.collisionDetectionMode = ballTransform.GetComponent<Rigidbody>().collisionDetectionMode;

        if(_lastForce != force)
        {
            rb.AddForce(force, forceMode);
            while(points.Count < 128)
            {
                var overlaps = Physics.OverlapSphere(_currentSimulationBall.transform.position, collider.radius);
                if(overlaps.Length != 0)
                {
                    Debug.Log(overlaps[0].name);
                    break;
                }
                else Debug.Log(overlaps.Length);

                points.Add(_currentSimulationBall.transform.position);
                _physicsScene.Simulate(Time.fixedDeltaTime);
            }
        }

        _lastForce = force;

        return points.ToArray();
    }

    void Start()
    {
        Instance = this;
        CreatePhysicsSimulationScene();

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

        // FindObjectOfType<ObjectSpawner>().SpawnBall(Network.accountData, new Vector3(12.369f, 1.23f, -1.345f));

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
