using System;
using System.Collections.Generic;
using System.Linq;
using TheSanctuary;
using UnityEngine;

//  данный класс представляет собой логику спавнера объектов  на площадке
//  и удаления их во время сетевых событий
//
//  скрипт написан недавно, по этому класс "слишком много на себя берёт"

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField]
    public GameObject MyBall;
    [SerializeField]
    public Material MyBallMaterial;
    [SerializeField]
    public GameObject NetworkBall;

    [SerializeField]
    public Texture2D[] Patterns;

    public static string RoomCreatorID;
    private List<GameObject> _networkBallArray = new List<GameObject>();
    private GUI _gui;

    void Start()
    {
        _gui = FindObjectOfType<GUI>();

        Network.OnGetRoomDataEvent += OnGetRoomData;
        Network.OnRoomConnectionEvent += OnRoomConnection;
        Network.OnRoomDisconnectionEvent += OnRoomDisconnection;
    }

    public void SetBallMaterial(Outlook outlookData)
    {
        Color baseColor;
        Color linesColor;

        ColorUtility.TryParseHtmlString(outlookData.BaseColor, out baseColor);
        ColorUtility.TryParseHtmlString(outlookData.LinesColor, out linesColor);

        MyBallMaterial.SetColor(BallCustomize.baseColorID, baseColor);
        MyBallMaterial.SetColor(BallCustomize.linesColorID, linesColor);

        if(outlookData.UsePattern)
        {
            var patternTexture = Patterns.ToList().Find(x => x.name == outlookData.PatternName);
            MyBallMaterial.SetInt(BallCustomize.usePatternID, 1);
            MyBallMaterial.SetTexture(BallCustomize.patternTextureID, patternTexture);
        }
        else MyBallMaterial.SetInt(BallCustomize.usePatternID, 0);
    }

    private void OnRoomDisconnection(string playerSessionId)
    {
        try
        {
            var disconnectPlayerBall = _networkBallArray.Find(x => x.name == playerSessionId);
            var networkBallScript = disconnectPlayerBall.GetComponent<NetworkBall>();

            _gui.ShowPopUpMessage($"player {networkBallScript.PlayerName} leave this room", Color.yellow, PopUpMessageType.Info);
            if(playerSessionId == RoomCreatorID)
            {
                _gui.ShowPopUpMessage($"room closed", Color.red, PopUpMessageType.Error);
                Network.GameMode = null;
                Network.inRoom = false;
            }
            
            _gui.RemovePlayerNickname(networkBallScript.PlayerName);
            
            _networkBallArray.Remove(disconnectPlayerBall);
            Destroy(disconnectPlayerBall);
        }
        catch(Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private void OnRoomConnection(string playerSessionId, PersonalData playerAccount)
    {
        try
        {
            SpawnNetworkPlayerBall(playerSessionId, playerAccount, false);
            _gui.ShowPopUpMessage($"player {playerAccount.login} connected to room", Color.yellow, PopUpMessageType.Info);
        }
        catch(Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private void OnGetRoomData(string creatorID, Dictionary<string, PersonalData> accounts, List<bool> readyStatusList)
    {
        try
        {
            RoomCreatorID = creatorID;

            var playersSessionIDs = accounts.Keys.ToList();
            var playersAccounts = accounts.Values.ToList();

            for(int i = 0; i < playersAccounts.Count; i++)
            {
                SpawnNetworkPlayerBall(playersSessionIDs[i], playersAccounts[i], readyStatusList[i]);
            }

            // скорее всего, этот код нужно выполнить по событию OnFadeOut в GameMode
            _gui.ShowPopUpMessage($"you have joined the {playersAccounts.First().login}'s room",
                Color.yellow,
                PopUpMessageType.Info);
        }
        catch(Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private void SpawnNetworkPlayerBall(string playerSessionID, PersonalData playerData, bool playerReadyStatus)
    {
        
        var newBall = Instantiate(NetworkBall, new Vector3(0, 1.5f, 0), Quaternion.identity);
        newBall.GetComponent<NetworkBall>().SetNetworkBallData(playerSessionID, playerData, playerReadyStatus);

        _gui.AddPlayerNickname(playerData.login);

        _networkBallArray.Add(newBall);
    }

    public void SpawnBall(Account myData, Vector3 position)
    {
        var newMyBall = Instantiate(MyBall, position, Quaternion.identity);
        newMyBall.GetComponent<BallLogic>().SetBallOutlook(MyBallMaterial);

        if(FindObjectsOfType<BallLogic>().Length <= 1)
        {
            var backgroundBall = GameObject.Find("Background Ball");
            backgroundBall.GetComponent<Renderer>().material = MyBallMaterial;
        }
    }
}
