using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using Photon.Pun.Demo.Cockpit;

public class Launcher : MonoBehaviourPunCallbacks
{

    public static Launcher instance;
    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] Transform rlC;
    [SerializeField] Transform plC;
    [SerializeField] GameObject RoomListPrefab;
    [SerializeField] GameObject playerListPrefab;
    [SerializeField] GameObject startGameButton;
    [SerializeField] GameObject RoomSelectUI;
    public int levelID = 1;
    public int playerCount = 1;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Connecting to Master");
        PhotonNetwork.ConnectUsingSettings();
    }

    private void Awake()
    {
        instance = this;
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    public override void OnJoinedLobby()
    {
        MenuManager.instance.OpenMenu("Title");
        Debug.Log("Joined lobby");
        PhotonNetwork.NickName = "Player" + Random.Range(0, 1000).ToString("0000");
    }

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInputField.text))
        {
            return;
        }
        PhotonNetwork.CreateRoom(roomNameInputField.text);
        roomNameInputField.text = "";
        MenuManager.instance.OpenMenu("Loading");
        playerCount = 1;
    }

    public override void OnJoinedRoom()
    { 
        MenuManager.instance.OpenMenu("Room");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        foreach(Transform child in plC)
        {
            Destroy(child.gameObject);
        }
        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            Instantiate(playerListPrefab, plC).GetComponent<PlayerListItem>().SetUp(players[i]);
        }

        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
        RoomSelectUI.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
         startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Room Creation Failed" + message;
        MenuManager.instance.OpenMenu("Error Menu");
    }

    public void LeaveRoom()
    {
        GameManager.Instance.started = false;
        PhotonNetwork.LeaveRoom();
        MenuManager.instance.OpenMenu("Loading");
    }

    public override void OnLeftRoom()
    {
        GameManager.Instance.started = false;
        MenuManager.instance.OpenMenu("Title");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach(Transform trans in rlC)
        {
            Destroy(trans.gameObject);
        }
        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].RemovedFromList)
                continue;
            Instantiate(RoomListPrefab, rlC).GetComponent<RoomListItem>().SetUp(roomList[i]);
        }
    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.instance.OpenMenu("Loading");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        playerCount++;
        Instantiate(playerListPrefab, plC).GetComponent<PlayerListItem>().SetUp(newPlayer);
    }

    public void StartGame()
    {
        if (playerCount <= 1)
            return;
        GameManager.Instance.playerCount = playerCount;
        GameManager.Instance.resetTimer();
        RoomManager.instance.CallRPC(playerCount);
        GameManager.Instance.started = true;
        PhotonNetwork.LoadLevel(levelID);
    }

    public void GetLevelID(string id)
    {
        levelID = int.Parse(id);
    }

    public void GetLevelIDInt(int id)
    {
        levelID = id;
    }
}
