using System.Linq;
using TheSanctuary.Interfaces;
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
    public bool setControl = true;
    [HideInInspector]
    public bool setMovingControl = true;
    [HideInInspector]
    public bool gameStarted;

    private Vector3 _startCameraPosition;
    private Quaternion _startCameraRotation;

    private Cloth[] _netsClothes;

    public List<ClothSphereColliderPair> clothColliders = new List<ClothSphereColliderPair>();
    private BackgroundCameraAnimation _bgCamera;

    public IGameMode currentGameMode;

    void Start()
    {
        Instance = this;

        _netsClothes = GameObject.FindObjectsOfType<Cloth>();
        cameraMain = Camera.main.gameObject;
        
        _startCameraPosition = cameraMain.transform.position;
        _startCameraRotation = cameraMain.transform.rotation;

        if(PlayerDataHandler.Load()) Debug.Log($"Данные успешно загружены: {PlayerDataHandler.playerData.username}, {PlayerDataHandler.playerData.avgAccuracy}%");
        InitializationGame();
    }

    public void InitializationGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        gameStarted = true;
    }

    public void AddColliderForNet(SphereCollider collider)
    {
        clothColliders.Add(new ClothSphereColliderPair(collider, collider));
        foreach(var cloth in _netsClothes)
        {
            cloth.sphereColliders = clothColliders.ToArray();
        }
    }

    public void RemoveColliderForNet(SphereCollider collider)
    {
        clothColliders.Remove(new ClothSphereColliderPair(collider, collider));
        foreach(var cloth in _netsClothes)
        {
            cloth.sphereColliders = clothColliders.ToArray();
        }
    }

    private void OnApplicationQuit()
    {
        PlayerDataHandler.Save();   
    }
}
