using UnityEngine;
using TheSanctuary;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Client;
using System;


//  данный класс ответственен за всё, что связано с клиент-серверным
//  взаимодействием в игре. здесь осуществляется:
//      -   подключение к серверу;
//      -   подписка на его события;
//      -   создание внутреигровых событий (посредством событий типа Action<T>);
//      -   формирование запросов на события клиента.
//  серверная часть работает на ASP.NET Core используя SignalR.


public class Network : MonoBehaviour
{
    public static Account accountData = new Account()
    {
        
    };

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
    public static Outlook BallOutlook;

    void Start()
    {
        StartNetworkConnection();
        if(_connection.State == HubConnectionState.Connected)
            SendServerData("ServerGetRoomsList");
    }    

    //  ниже описаны метод с его расширениями по вызову событий на сервере
    //  с передачей данных с предварительой сериализацией

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

        //  события игровых режимов, по моей задумке, обмениваются массивами строк,
        //  т.к. аргументы внутри того или иного режима будут отличаться друг от друга
        //  по количеству и содержанию вводных данных, по этому предусмотрен вариант
        //  создания масства из сериализованных данных

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

    //  блок описания подключения к серверу и подписки на его события

    public async void StartNetworkConnection()
    {
        _connection = new HubConnectionBuilder()
        .WithUrl("http://192.168.0.159:5000/mainHub")
        .Build();

        //  событие по попытке авторизации клиента
        _connection.On<string, string>("PlayerAuthorization", (PersonalData, id) =>
        {
            if(PersonalData != string.Empty)
            {
                accountData = JsonConvert.DeserializeObject<Account>(PersonalData);
                SessionID = id;

                Outlook ballOutlook = new Outlook
                {
                    BaseColor = accountData.baseColor,
                    LinesColor = accountData.linesColor,
                    UsePattern = accountData.usePattern,
                    PatternName = accountData.patternName
                };

                BallOutlook = ballOutlook;

                FindObjectOfType<ObjectSpawner>().SetBallMaterial(BallOutlook);
            }
        });

        //  событие, вызванное при ошибке ввода логина/пароля
        _connection.On("PlayerRegistrationException", ()=> accountRegistrationState = false);

        //  событие по отправленному клиентом запросу на количество созданных на
        //  текущий момент игровых комнат на сервере
        _connection.On<string>("PlayerOnRoomsListRequest", rooms =>
        {
            if(rooms != string.Empty)
            {
                var roomsData = JsonConvert.DeserializeObject<RoomBase[]>(rooms);
                foreach(var room in roomsData) RoomsList.Add(room as Room);
                OnRoomsListRequestEvent?.Invoke(roomsData);
            }
        });

        //  событие по запросу клиентом данных о треках для плеера:
        //  если данные на клиенте не соответствуют данным на серевере -
        //  клиент запрашивает недостоющие треки или же удаляет те, что
        //  не существуют на сервере
        _connection.On<string>("PlayerGetTracksData", x =>
        {
            var data = JsonConvert.DeserializeObject<Track[]>(x);
            Debug.Log(data.Length);
        });

        //  событие по запросу клиента на создание новой игровой комнаты
        _connection.On<string>("PlayerOnNewRoomCreated", room =>
        {
            if(room != string.Empty)
            {
                var roomData = JsonConvert.DeserializeObject<RoomBase>(room);

                if(roomData.CreatorSessionID == SessionID)
                {
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
        });

        //  событие по подлючению в игровую комнату нового игрока
        //  (вызывается в случае если клиент находится в игровой комнате)
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

        //  событие по запросу игроком данным о комнате, к которой он подключился
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

        //  событие по смене одним из игроков статуса готовности к игре
        _connection.On<string>("PlayerOnReadyStatusChanged", playerSessionId => OnReadyStatusChangedEvent?.Invoke(playerSessionId));
        //  событие по закрытию игровой комнаты
        _connection.On<string>("PlayerOnRoomClosed", roomID => OnRoomClosedEvent?.Invoke(int.Parse(roomID)));
        //  событие по открытию игровой комнаты
        _connection.On<string>("PlayerOnRoomOpen", roomData =>
        {
            var room = JsonConvert.DeserializeObject<RoomBase>(roomData);
            OnRoomOpenEvent?.Invoke(room);
        });
        //  событие по отключению игрока от игровой комнаты
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
        //  событие по инициализации игрового режима
        _connection.On<string>("PlayerOnGameInitialization", e =>
        {
            var eventArgs = JsonConvert.DeserializeObject<string[]>(e);
            OnGameInitializationEvent?.Invoke(eventArgs);
        });
        //  событие по броску игроками мяча
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
        //  событие по движению игроков
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
        //  событие по попаданию мячом в кольцо
        _connection.On<string>("PlayerOnBallScoreGetting", e =>
        {
            var eventArgs = JsonConvert.DeserializeObject<string[]>(e);
            OnBallScoreGettingEvent?.Invoke(eventArgs);
        });
        //  событие по соприкасновению мяча с паркетом
        _connection.On<string>("PlayerOnBallParketGetting", e =>
        {
            var eventArgs = JsonConvert.DeserializeObject<string[]>(e);
            OnBallParketGettingEvent?.Invoke(eventArgs);
        });
        //  событие по окончании игры
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


//  класс комнаты для того, чтобы отобразить ее данные
//  в списке игровых комнат
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
