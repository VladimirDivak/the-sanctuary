using System.Collections.Generic;
using TheSanctuary;
using UnityEngine;
using TMPro;

//  логика поведения списка игровых комнат на сервере
public class RoomList : MonoBehaviour
{
    private protected GameObject _noActiveRooms;

    [SerializeField]
    public GameObject RoomStateLine;
    public static List<GameObject> RoomLinesArray {get; private set;} = new List<GameObject>();

    void Start()
    {
        _noActiveRooms = GameObject.Find("NoActiveRooms_Text");

        Network.OnRoomsListRequestEvent += OnGetRoomsList;
        Network.OnNewRoomCreatedEvent += OnRoomCreated;
        Network.OnRoomOpenEvent += OnRoomOpen;
        Network.OnRoomClosedEvent += OnRoomClosed;
        Network.OnRoomDisconnectionEvent += OnRoomDisconnect;
    }

    private void OnRoomOpen(RoomBase room) => OnRoomCreated(room);
    private void OnRoomClosed(int roomID)
    {
        Destroy(RoomLinesArray[roomID]);
        RoomLinesArray.Remove(RoomLinesArray[roomID]);

        if(RoomLinesArray.Count == 0) _noActiveRooms.SetActive(true);
    }

    private void OnRoomDisconnect(string playerSessionID)
    {
        var line = RoomLinesArray.Find(x => x.GetComponent<RoomStateLine>().CreatorSessionID == playerSessionID);
        if(line != null)
        {
            RoomLinesArray.Remove(line);
            Destroy(line);

            if(RoomLinesArray.Count == 0) _noActiveRooms.SetActive(true);
        }
    }

    private void OnGetRoomsList(RoomBase[] roomData)
    {
        foreach(var room in roomData)
        {
            var gameModeName = room.GameModeName;
            var playersCounter = room.PlayersCounter;

            OnRoomCreated(room);
        }

        Network.OnRoomsListRequestEvent -= OnGetRoomsList;
    }

    private void OnRoomCreated(RoomBase roomData)
    {
        if(roomData.CreatorSessionID != Network.SessionID)
        {
            var gameModeName = roomData.GameModeName;

            if(_noActiveRooms.activeSelf) _noActiveRooms.SetActive(false);

            var stateLine = Instantiate(RoomStateLine, Vector2.zero, Quaternion.identity);
            stateLine.GetComponent<RoomStateLine>().CreatorSessionID = roomData.CreatorSessionID;

            var modeName = stateLine.GetComponentInChildren<TMP_Text>();
            modeName.text = gameModeName.ToUpper();

            var stateLineRect = stateLine.GetComponent<RectTransform>();
            stateLineRect.SetParent(GetComponent<RectTransform>());

            RoomLinesArray.Add(stateLine);
        }
    }

    public void OnRoomPlayersValueChanged(int roomId)
    {
        var roomStateLine = RoomLinesArray[roomId];

        var textArray = roomStateLine.GetComponentsInChildren<TMP_Text>();
        textArray[1].text = (int.Parse(textArray[1].text) + 1).ToString();
    }
}
