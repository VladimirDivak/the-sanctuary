using System.Collections.Generic;
using UnityEngine;
using TheSanctuary;
using System;


// Класс представляет собой реализацию игрового режима 33
public class ThirtyThreeMode : MonoBehaviour, IMode
{
    public int MaxPlayers { get; set; }
    public int MaxScores { get; set; }
    public Dictionary<string, Player> Players { get; set; } = new Dictionary<string, Player>();

    void Start()
    {
        MaxPlayers = 5;
        MaxScores = 33;

        //  подписка на событие подключения нового игрока к текущей игровой комнате
        Network.OnRoomConnectionEvent += OnPlayerConnection;
        //  подписка на событие изменения статуса готовности игрока  
        Network.OnReadyStatusChangedEvent += OnPlayerReadyStatusChanged;
        //  подписка на событие отключения игрока от игровой комнаты
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


    //  нижепредставленные методы являются реализацией интерфейса IMode
    public string[] OnBallParketGetting(string[] methodArgs)
    {
        //  TODO: события при соприкасновения мяча с паркетом
        throw new NotImplementedException();
    }

    public string[] OnBallScoreGetting(string[] methodArgs)
    {
        //  TODO: события при попадании игроками мячом в кольцо
        throw new NotImplementedException();
    }

    public string[] OnGameEnding(string[] methodArgs)
    {
        //  TODO: последовательность действий по окончании игры
        throw new NotImplementedException();
    }

    public string[] OnGameInitialization(string[] methodArgs)
    {
        //  TODO: события при инициализации игры
        throw new NotImplementedException();
    }


    //  эти методы нужно выделить в отдельный интерфейс, т.к.
    //  они одинаковы во всех игровых режимах
    public Force OnBallThrowning(string playerSessionId, Force throwForceData)
    {
        throw new NotImplementedException();
    }

    public PlayerTransform OnPlayerMoving(string playerSessionId, PlayerTransform transformData)
    {
        throw new NotImplementedException();
    }
}