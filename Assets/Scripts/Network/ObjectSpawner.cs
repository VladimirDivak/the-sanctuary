using System;
using System.Collections.Generic;
using System.Linq;
using TheSanctuary;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField]
    public GameObject NetworkBall;

    public static string RoomCreatorID;
    private List<GameObject> _networkBallArray = new List<GameObject>();

    void Start()
    {
        Network.OnGetRoomDataEvent += OnGetRoomData;
        Network.OnRoomConnectionEvent += OnRoomConnection;
        Network.OnRoomDisconnectionEvent += OnRoomDisconnection;
    }

    private void OnRoomDisconnection(string playerSessionId)
    {
        try
        {
            var guiScript = FindObjectOfType<GUI>();
            var disconnectPlayerBall = _networkBallArray.Find(x => x.name == playerSessionId);
            var networkBallScript = disconnectPlayerBall.GetComponent<NetworkBall>();

            guiScript.ShowPopUpMessage($"player {networkBallScript.PlayerName} leave this room", Color.yellow, PopUpMessageType.Info);
            if(playerSessionId == RoomCreatorID)
            {
                guiScript.ShowPopUpMessage($"room closed", Color.red, PopUpMessageType.Error);
                Network.GameMode = null;
                Network.inRoom = false;
            }
            
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
            SpawnNetworkPlayer(playerSessionId, playerAccount, false);
            FindObjectOfType<GUI>().ShowPopUpMessage($"player {playerAccount.login} connected to room", Color.yellow, PopUpMessageType.Info);
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
                SpawnNetworkPlayer(playersSessionIDs[i], playersAccounts[i], readyStatusList[i]);
            }

            // скорее всего, этот код нужно выполнить по событию OnFadeOut в GameMode
            FindObjectOfType<GUI>().ShowPopUpMessage($"you have joined the {playersAccounts.First().login}'s room",
                Color.yellow,
                PopUpMessageType.Info);
        }
        catch(Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private void SpawnNetworkPlayer(string playerSessionID, PersonalData playerData, bool playerReadyStatus)
    {
        Debug.Log(playerData.baseColor);
        Debug.Log(playerData.linesColor);

        var newBall = Instantiate(NetworkBall, new Vector3(0, 1, 0), Quaternion.identity);
        newBall.GetComponent<NetworkBall>().SetNetworkBallData(playerSessionID, playerData, playerReadyStatus);

        _networkBallArray.Add(newBall);
    }
}
