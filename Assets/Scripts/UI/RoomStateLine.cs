using UnityEngine;
using TMPro;
using System;

public class RoomStateLine : MonoBehaviour
{
    [SerializeField]
    public int roomID { get; private set; }
    [SerializeField]
    public string CreatorSessionID;

    void Start()
    {
        roomID =  RoomList.RoomLinesArray.IndexOf(RoomList.RoomLinesArray.Find(x =>
            x.name.Contains(this.name)));
    }

    public void OnEnterButtonPressed()
    {
        try
        {
            Network network = FindObjectOfType<Network>();

            FindObjectOfType<UserInterface>().OnGameMenuExit();
            GUI.ShowGameRoomsPanel(false);
            GUI.ShowGameUI(true);

            network.SendServerData("ServerOnRoomConnection", false,
                Network.accountData.login,
                roomID);
        }
        catch(Exception error)
        {
            Debug.LogError(error);
        }
    }
}
