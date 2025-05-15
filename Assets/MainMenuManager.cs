using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor.Search;
using Unity.VisualScripting;

public class MainMenuManager : NetworkBehaviour
{
    public static MainMenuManager Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        if (NetworkManager.Singleton != null)
        {
            DontDestroyOnLoad(NetworkManager.Singleton.gameObject);
        }
    }

    public void OnHostButtonClicked()
    {
        CreateLobby();

        NetworkManager.Singleton.StartHost();

        SceneManager.LoadScene("LobbyScene");
    }

    public void OnJoinButtonClicked()
    {
        QuickJoinLobby();

        NetworkManager.Singleton.StartClient();

        SceneManager.LoadScene("LobbyScene");
    }

    public void ListLobbiesButton()
    {
        ListLobbies();
    }

    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    private string playerName;
    public void SetJoinedLobby(Lobby lobby) => joinedLobby = lobby;
    public Lobby GetJoinedLobby() => joinedLobby;
    public bool IsHosting() => hostLobby != null;
    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        playerName = "Josh" + UnityEngine.Random.Range(10, 99);
        Debug.Log(playerName);
    }

    private void Update()
    {
        HandleLobbyHeartBeat();
        HandleLobbyPollForUpdates();
    }

    private async void HandleLobbyHeartBeat()
    {
        if (hostLobby != null) {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f) {
                float heartbeatTimerMax = 15;
                heartbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    private async void HandleLobbyPollForUpdates() 
    {
        if (joinedLobby != null) {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f) {
                float lobbyUpdateTimerMax = 1.1f;
                lobbyUpdateTimer = lobbyUpdateTimerMax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;
            }
        }
    }

    private async void CreateLobby()
    {
        try {
            string lobbyName = "MyLobby";
            int maxPlayers = 4;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions{
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>{
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "CaptureTheFlag")}
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            hostLobby = lobby;
            joinedLobby = hostLobby;

            PrintPlayer(hostLobby);

            Debug.Log("Created Lobby! " + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
        
    }

    private async void ListLobbies()
    {
        try {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions{
                Count = 25,
                Filters = new List<QueryFilter> {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                },
                Order = new List<QueryOrder> {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();

            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results) {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Data["GameMode"].Value);
            }
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }


        private async void JoinLobbyByCode(string lobbyCode) {
            try {
                JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions{
                    Player = GetPlayer()
                };
                Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
                joinedLobby = lobby;
                Debug.Log("Joined Lobby with code " + lobbyCode);

                PrintPlayer(joinedLobby);
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }

        private async void QuickJoinLobby() {
            try{
                await LobbyService.Instance.QuickJoinLobbyAsync();
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }

        private Player GetPlayer() {
            return new Player{
                Data = new Dictionary<string, PlayerDataObject>{
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
                }
            };
        }

        private void PrintPlayers()
        {
            PrintPlayer(joinedLobby);
        }
        private void PrintPlayer(Lobby lobby) {
            Debug.Log("Players in Lobby " + lobby.Name + " " + lobby.Data["GameMode"].Value + " " + lobby.Data["Map"].Value);
            foreach (Player player in lobby.Players) {
                Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
            }
        }

        private async void UpdateLobbyGameMode(string gameMode) {
            try {
                hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions {
                    Data = new Dictionary<string, DataObject>{
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public,gameMode) },
                    { "Map", new DataObject(DataObject.VisibilityOptions.Public, "de_dust2")}
                    }
                });
                joinedLobby = hostLobby;

                PrintPlayer(hostLobby);
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }

        private async void UpdatePlayerName(string newPlayerName) {
            try{
                playerName = newPlayerName;
                await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions {
                    Data = new Dictionary<string, PlayerDataObject> {
                        { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
                    }
                });
            } catch (LobbyServiceException e){
                Debug.Log(e);
            }
        }

        private async void LeaveLobby() {
            try{
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
        private async void KickPlayer() {
            try{
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[1].Id);
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }

        private async void MigrateLobbyHost() {
            try {
                hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby. Id, new UpdateLobbyOptions {
                        HostId = joinedLobby.Players[1].Id
                });

                joinedLobby = hostLobby;
                PrintPlayer(hostLobby);
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }

        private async void DeleteLobby()
        {
            try {
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
            } catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
}
