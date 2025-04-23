using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    UIManager uiManager;
    public TMP_InputField roomNameInputField;
    private Dictionary<string, RoomInfo> cachedRoomList;
    public GameObject roomListParentGameobject;
    public GameObject roomListEntryPrefab;
    private Dictionary<string, GameObject> roomListGameobjects;
    public GameObject playerNamePrefab;
    public GameObject playerNameParent;
    private Dictionary<int, GameObject> playerListGameobjects;
    public GameObject startGameButton;
    

    private void Start()
    {
        uiManager = GetComponent<UIManager>();
        cachedRoomList = new Dictionary<string, RoomInfo>(); 
        roomListGameobjects = new Dictionary<string, GameObject>();

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public void OnStartGameButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("GameScene");
        }
    }

    public void OnRoomCreateButtonClicked()
    {
        string roomName = roomNameInputField.text;

        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "Room " + Random.Range(1000,10000);
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4;

        PhotonNetwork.CreateRoom(roomName,roomOptions);
        uiManager.GoToRoom();
    }


    public override void OnCreatedRoom()
    {
        Debug.Log("Room" + PhotonNetwork.CurrentRoom.Name+ " is created.");
    }
 
    public override void OnJoinedRoom()
    {
        uiManager.GoToRoom();
        Debug.Log(PhotonNetwork.LocalPlayer.NickName+ " joined to "+ "Room " + PhotonNetwork.CurrentRoom.Name );

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            startGameButton.SetActive(true);
        }
        else
        {
            startGameButton.SetActive(false);
        }

        if (playerListGameobjects==null)
        {
            playerListGameobjects = new Dictionary<int, GameObject>();
        } 

        UpdatePlayerList();

    }
    public override void OnLeftRoom()
    {
        uiManager.GoToMainMenu();
        foreach (GameObject playerListGameobject in playerListGameobjects.Values)
        {
            Destroy(playerListGameobject);
        }

        playerListGameobjects.Clear();
        playerListGameobjects = null;

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            startGameButton.SetActive(true);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("New player joined: " + newPlayer.NickName);
        UpdatePlayerList();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Player left: " + otherPlayer.NickName);
        UpdatePlayerList();
    }

    private void UpdatePlayerList()
    {
        ClearPlayerList();
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject playerListGameobject = Instantiate(playerNamePrefab);
            playerListGameobject.transform.SetParent(playerNameParent.transform);
            playerListGameobject.transform.localScale = Vector3.one;
            playerListGameobject.transform.Find("PlayerNameText").GetComponent<TMP_Text>().text = player.NickName;
            playerListGameobjects[player.ActorNumber] = playerListGameobject;

            if (player.ActorNumber==PhotonNetwork.LocalPlayer.ActorNumber)
            {
                playerListGameobject.transform.Find("PlayerIndicator").gameObject.SetActive(true);
            }
            else
            {
                playerListGameobject.transform.Find("PlayerIndicator").gameObject.SetActive(false);
            }
        }
    }

    private void ClearPlayerList()
    {
        foreach (var playerListGameobject in playerListGameobjects.Values)
        {
            Destroy(playerListGameobject);
        }
        playerListGameobjects.Clear(); 
    }

    public void OnShowRoomListButtonClicked()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
        uiManager.GoToLobbySession();
    }

    public void OnJoinRoomButtonClicked(string _roomName)
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        PhotonNetwork.JoinRoom(_roomName);
    }

    public void OnLeaveRoomButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
        uiManager.GoToMainMenu(); 
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {   
        ClearRoomListView();

        foreach (RoomInfo room in roomList)
        {
            Debug.Log(room.Name);
            if (!room.IsOpen || !room.IsVisible|| room.RemovedFromList )
            {
                if (cachedRoomList.ContainsKey(room.Name))
                {
                    cachedRoomList.Remove(room.Name);
                }
            }
            else
            {
                //update cachedRoom list
                if (cachedRoomList.ContainsKey(room.Name))
                {
                    cachedRoomList[room.Name] = room;
                }
                //add the new room to the cached room list
                else
                {
                    cachedRoomList.Add(room.Name, room);
                }
            }
        }
        foreach (RoomInfo room in cachedRoomList.Values)
        {
            GameObject roomListEntryGameobject = Instantiate(roomListEntryPrefab);
            roomListEntryGameobject.transform.SetParent(roomListParentGameobject.transform);
            roomListEntryGameobject.transform.localScale = Vector3.one;

            roomListEntryGameobject.transform.Find("RoomNameText").GetComponent<TMP_Text>().text = room.Name;
            roomListEntryGameobject.transform.Find("RoomPlayersText").GetComponent<TMP_Text>().text = room.PlayerCount+ " / "+ room.MaxPlayers;
            roomListEntryGameobject.transform.Find("JoinRoomButton").GetComponent<Button>().onClick.AddListener(()=>OnJoinRoomButtonClicked(room.Name));

            roomListGameobjects.Add(room.Name,roomListEntryGameobject);
        }

    }

    void ClearRoomListView()
    {
        foreach (var roomListGameobject in roomListGameobjects.Values)
        {
            Destroy(roomListGameobject);
        }

        roomListGameobjects.Clear();
    }

    

}
