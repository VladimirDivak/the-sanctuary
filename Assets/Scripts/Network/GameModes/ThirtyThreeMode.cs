using System.Collections.Generic;
using UnityEngine;
using TheSanctuary;
using System;

public class ThirtyThreeMode : MonoBehaviour, IMode
{
    public int MaxPlayers { get; set; }
    public int MaxScores { get; set; }
    public Dictionary<string, Player> Players { get; set; } = new Dictionary<string, Player>();

    void Start()
    {
        MaxPlayers = 5;
        MaxScores = 33;

        Network.OnRoomConnectionEvent += OnPlayerConnection;
        Network.OnReadyStatusChangedEvent += OnPlayerReadyStatusChanged;
        Network.OnRoomDisconnectionEvent += OnPlayerDisconnection;
    }

    private void OnPlayerConnection(string playerSessionID, PersonalData playerData)
    {
        Players.Add(playerSessionID, new Player(playerData));
    }

    private void OnPlayerReadyStatusChanged(string playerSessionID)
    {
        Players[playerSessionID].ReadyStatus = !Players[playerSessionID].ReadyStatus;
    }

    private void OnPlayerDisconnection(string playerSessionID)
    {
        Players.Remove(playerSessionID);
    }

    public string[] OnBallParketGetting(string[] methodArgs)
    {
        throw new NotImplementedException();
    }

    public string[] OnBallScoreGetting(string[] methodArgs)
    {
        throw new NotImplementedException();
    }

    public string[] OnGameEnding(string[] methodArgs)
    {
        throw new NotImplementedException();
    }

    public string[] OnGameInitialization(string[] methodArgs)
    {
        throw new NotImplementedException();
    }

    public Force OnBallThrowning(string playerSessionId, Force throwForceData)
    {
        throw new NotImplementedException();
    }

    public PlayerTransform OnPlayerMoving(string playerSessionId, PlayerTransform transformData)
    {
        throw new NotImplementedException();
    }
}