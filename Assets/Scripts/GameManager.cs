using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameObject MainCamera;
    public static GameObject PlayerController;
    public static GameObject ShootPoint;

    private GUI GUIScript;

    public static bool SetControl = true;
    public static bool SetMovingControl = true;
    public static bool GameStarted;

    private Vector3 StartCameraPosition;
    private Quaternion StartCameraRotation;

    private Cloth[] NetsClothes;

    public List<ClothSphereColliderPair> ClothColliders = new List<ClothSphereColliderPair>();
    private BackgroundCameraAnimation BGCamera;

    private UserInterface UI;

    void Start()
    {
        GUIScript = FindObjectOfType<GUI>();

        UI = GameObject.FindObjectOfType<UserInterface>();
        UI.OnGameMenuEnter();

        NetsClothes = GameObject.FindObjectsOfType<Cloth>();

        MainCamera = GameObject.Find("PlayerController/Main Camera");
        StartCameraPosition = MainCamera.transform.position;
        StartCameraRotation = MainCamera.transform.rotation;

        PlayerController = GameObject.Find("PlayerController");
        PlayerController.SetActive(false);

        BGCamera = GameObject.FindObjectOfType<BackgroundCameraAnimation>();
        BGCamera.StartBackgroundCameraMoving();

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

        UI.OnGameMenuExit();
        GUI.ShowRegistrationPanel(false);
        GUI.ShowGameUI(true);

        GameObject.FindObjectOfType<CameraRaycast>().OnGameInit();

        var Balls = GameObject.FindGameObjectsWithTag("Ball");
        foreach(var Ball in Balls)
        {
            ClothColliders.Add(new ClothSphereColliderPair(Ball.GetComponent<SphereCollider>(), Ball.GetComponent<SphereCollider>()));
        }
        

        foreach(var cloth in NetsClothes)
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
            if(UI.AbleToShowMenu)
            {
                UI.OnGameMenuEnter();
            }
            else
            {
                UI.OnGameMenuExit();
            }
        }
    }
}
