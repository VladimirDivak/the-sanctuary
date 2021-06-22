using UnityEngine;
using TheSanctuary;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Client;
using System;

public class Network : MonoBehaviour
{
    public static Account accountData = new Account();

    public static IMode GameMode;
    public static bool inRoom;
    public static string SessionID;

    public static bool accountRegistrationState = true;

    public static List<Room> RoomsList = new List<Room>();
    
    public static event Action<RoomBase[]> OnRoomsListRequestEvent;
    public static event Action<RoomBase> OnNewRoomCreatedEvent;
    public static event Action<string, PersonalData> OnRoomConnectionEvent;
    public static event Action<string, Dictionary<string, PersonalData>, List<bool>> OnGetRoomDataEvent;
    public static event Action<string> OnReadyStatusChangedEvent;
    public static event Action<RoomBase> OnRoomOpenEvent;
    public static event Action<int> OnRoomClosedEvent;
    public static event Action<string> OnRoomDisconnectionEvent;

    public static event Action<string[]> OnGameInitializationEvent;
    public static event Action<string, Force> OnBallThrowingEvent;
    public static event Action<string[]> OnBallScoreGettingEvent;
    public static event Action<string[]> OnBallParketGettingEvent;
    public static event Action<string, PlayerTransform> OnBallMovingEvent;
    public static event Action<string[]> OnGameEndingEvent;

    private protected HubConnection _connection;

    void Start()
    {
        StartNetworkConnection();
        SendServerData("ServerGetRoomsList");
    }    

    public async void SendServerData(string serverEventName)
    {
        if(_connection.State == HubConnectionState.Disconnected) return;
        
        await _connection.SendAsync(serverEventName);
    }

    public async void SendServerData<T>(string serverEventName, T data)
    {
        if(_connection.State == HubConnectionState.Disconnected) return;

        string jsonData;
        if(data.GetType() == typeof(string)) jsonData = data as string;
        else jsonData = JsonConvert.SerializeObject(data);

        await _connection.SendAsync(serverEventName, jsonData);
    }

    public async void SendServerData<T1, T2>(string serverEventName, bool createArray, T1 data1, T2 data2)
    {
        if(_connection.State == HubConnectionState.Disconnected) return;

        var jsonArray = new string[2];

        string jsonData1;
        string jsonData2;

        if(data1.GetType() == typeof(string)) jsonData1 = data1 as string;
        else jsonData1 = JsonConvert.SerializeObject(data1);

        if(data2.GetType() == typeof(string)) jsonData2 = data2 as string;
        else jsonData2 = JsonConvert.SerializeObject(data2);

        if(createArray)
        {
            jsonArray[0] = jsonData1;
            jsonArray[1] = jsonData2;

            await _connection.SendAsync(serverEventName, jsonArray);
        }
        else await _connection.SendAsync(serverEventName, jsonData1, jsonData2);
    }

    public async void SendServerData<T1, T2, T3>(string serverEventName, bool createArray, T1 data1, T2 data2, T3 data3)
    {
        if(_connection.State == HubConnectionState.Disconnected) return;

        var jsonArray = new string[3];

        string jsonData1;
        string jsonData2;
        string jsonData3;

        if(data1.GetType() == typeof(string)) jsonData1 = data1 as string;
        else jsonData1 = JsonConvert.SerializeObject(data1);

        if(data2.GetType() == typeof(string)) jsonData2 = data2 as string;
        else jsonData2 = JsonConvert.SerializeObject(data2);

        if(data3.GetType() == typeof(string)) jsonData3 = data3 as string;
        else jsonData3 = JsonConvert.SerializeObject(data3);

        if(createArray)
        {
            jsonArray[0] = jsonData1;
            jsonArray[1] = jsonData2;
            jsonArray[2] = jsonData3;

            await _connection.SendAsync(serverEventName, jsonArray);
        }
        else await _connection.SendAsync(serverEventName, jsonData1, jsonData2, jsonData3);
    }

    public async void StartNetworkConnection()
    {
        _connection = new HubConnectionBuilder()
        .WithUrl("http://192.168.0.159:5000/mainHub")
        .Build();

        _connection.On<string, string>("PlayerAuthorization", (PersonalData, id) =>
        {
            if(PersonalData != string.Empty)
            {
                accountData = JsonConvert.DeserializeObject<Account>(PersonalData);
                SessionID = id;
            }
        });

        _connection.On("PlayerRegistrationException", ()=> accountRegistrationState = false);

        _connection.On<string>("PlayerOnRoomsListRequest", rooms =>
        {
            if(rooms != string.Empty)
            {
                var roomsData = JsonConvert.DeserializeObject<RoomBase[]>(rooms);
                foreach(var room in roomsData) RoomsList.Add(room as Room);
                OnRoomsListRequestEvent?.Invoke(roomsData);
            }
        });

        _connection.On<string>("PlayerGetTracksData", x =>
        {
            var data = JsonConvert.DeserializeObject<Track[]>(x);
            Debug.Log(data.Length);
            
            FindObjectOfType<AudioPlayerLogic>().UnpackTracksData(data);
        });

        _connection.On<string>("PlayerOnNewRoomCreated", room =>
        {
            if(room != string.Empty)
            {
                var roomData = JsonConvert.DeserializeObject<RoomBase>(room);

                if(roomData.CreatorSessionID == SessionID)
                {
                    FindObjectOfType<GUI>().ShowPopUpMessage("the room is created.\nexpect players", Color.yellow, PopUpMessageType.Message);
                    ObjectSpawner.RoomCreatorID = SessionID;

                    inRoom = true;
                    CreateNewRoom(roomData);
                    
                    switch(roomData.GameModeName)
                    {
                        case "Thirty Three":
                            GameMode = FindObjectOfType<ThirtyThreeMode>();
                            break;
                    }
                }

                OnNewRoomCreatedEvent?.Invoke(roomData);
            }
            else FindObjectOfType<GUI>().ShowPopUpMessage("no free places for a room", Color.red, PopUpMessageType.Error);
        });

        _connection.On<string, string>("PlayerOnConnection", (playerSessionId, playerAccountData) =>
        {
            try
            {
                var playerAccount = JsonConvert.DeserializeObject<PersonalData>(playerAccountData);
                var playerData = new Player(playerAccount);

                OnRoomConnectionEvent?.Invoke(playerSessionId, playerAccount);
            }
            catch(Exception ex)
            {
                Debug.LogError(ex);
            }
        });

        _connection.On<string, string, string, string>("PlayerGetRoomData", (creatorID, gameModeName, accountsData, playersReadyStatusData) =>
        {
            try
            {
                var accounts = JsonConvert.DeserializeObject<Dictionary<string, PersonalData>>(accountsData);
                var playersReadyStatus = JsonConvert.DeserializeObject<List<bool>>(playersReadyStatusData);

                switch(gameModeName)
                {
                    case "ThirtyThree":
                        GameMode = FindObjectOfType<ThirtyThreeMode>();
                        break;
                }
                inRoom = true;

                OnGetRoomDataEvent?.Invoke(creatorID, accounts, playersReadyStatus);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        });

        _connection.On<string>("PlayerOnReadyStatusChanged", playerSessionId => OnReadyStatusChangedEvent?.Invoke(playerSessionId));
        _connection.On<string>("PlayerOnRoomClosed", roomID => OnRoomClosedEvent?.Invoke(int.Parse(roomID)));

        _connection.On<string>("PlayerOnRoomOpen", roomData =>
        {
            var room = JsonConvert.DeserializeObject<RoomBase>(roomData);
            OnRoomOpenEvent?.Invoke(room);
        });

        _connection.On<string>("PlayerOnDisconnection", playerSessionId =>
        {
            try
            {
                OnRoomDisconnectionEvent?.Invoke(playerSessionId);
            }
            catch(Exception ex)
            {
                Debug.LogError(ex);
            }
        });
        
        _connection.On<string>("PlayerOnGameInitialization", e =>
        {
            var eventArgs = JsonConvert.DeserializeObject<string[]>(e);
            OnGameInitializationEvent?.Invoke(eventArgs);
        });

        _connection.On<string, string>("PlayerOnBallThrowing", (a1, a2) =>
        {
            try
            {
                var playerSessionId = a1;
                var throwForceData = JsonConvert.DeserializeObject<Force>(a2);

                OnBallThrowingEvent?.Invoke(playerSessionId, throwForceData);
            }
            catch(Exception ex)
            {
                Debug.LogError(ex);
            }
        });

        _connection.On<string, string>("PlayerOnBallMoving", (a1, a2) =>
        {
            try
            {
                var playerSessionId = a1;
                var playerTransform = JsonConvert.DeserializeObject<PlayerTransform>(a2);

                OnBallMovingEvent?.Invoke(playerSessionId, playerTransform);
            }
            catch(Exception ex)
            {
                Debug.LogError(ex);
            }
        });

        _connection.On<string>("PlayerOnBallScoreGetting", e =>
        {
            var eventArgs = JsonConvert.DeserializeObject<string[]>(e);
            OnBallScoreGettingEvent?.Invoke(eventArgs);
        });

        _connection.On<string>("PlayerOnBallParketGetting", e =>
        {
            var eventArgs = JsonConvert.DeserializeObject<string[]>(e);
            OnBallParketGettingEvent?.Invoke(eventArgs);
        });

        _connection.On<string>("PlayerOnGameEnding", e =>
        {
            var eventArgs = JsonConvert.DeserializeObject<string[]>(e);
            OnGameEndingEvent?.Invoke(eventArgs);
            inRoom = false;
        });

        await _connection.StartAsync();
    }

    async void OnApplicationQuit() => await _connection.StopAsync();

    public void CreateNewRoom(RoomBase roomData)
    {
        try
        {
            var room = new Room(roomData.ID, roomData.GameModeName, roomData.PlayersCounter, roomData.CreatorSessionID);
            RoomsList.Add(room);
        }
        catch(Exception ex)
        {
            Debug.LogError(ex);
        }
    }
}

public class Room : RoomBase
{
    public Room(int roomId, string modeName, int playersCounter, string creatorSessionId)
    {
        ID = roomId;

        GameModeName = modeName;
        PlayersCounter = playersCounter;
        CreatorSessionID = creatorSessionId;
    }
}
