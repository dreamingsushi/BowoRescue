using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using System;

public class LobbyUIManager : NetworkBehaviour
{
    public static LobbyUI Instance { get; private set; }
    [SerializeField] private Transform playerSingleTemplate;
    [SerializeField] private Transform container;
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;


    [Header("UI References")]
    public Button startGameButton;
    public Button leaveLobbyButton;


    private void Start()
    {
        LobbyManagerZK.Instance.OnJoinedLobby += UpdateLobby_Event;
        LobbyManagerZK.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;
        LobbyManagerZK.Instance.OnLobbyGameModeChanged += UpdateLobby_Event;
        LobbyManagerZK.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManagerZK.Instance.OnKickedFromLobby += LobbyManager_OnLeftLobby;

        Hide();

        startGameButton.onClick.AddListener(OnStartGamePressed);
        leaveLobbyButton.onClick.AddListener(OnLeaveLobbyPressed);
    }



    private void LobbyManager_OnLeftLobby(object sender, System.EventArgs e)
    {
        ClearLobby();
        Hide();
    }

    private void UpdateLobby_Event(object sender, LobbyManagerZK.LobbyEventArgs e)
    {
        UpdateLobby();
    }

    private void UpdateLobby()
    {
        UpdateLobby(LobbyManagerZK.Instance.GetJoinedLobby());
    }

    private void UpdateLobby(Lobby lobby)
    {
        ClearLobby();

        foreach (Player player in lobby.Players)
        {
            Transform playerCard = Instantiate(playerSingleTemplate, container);
            playerCard.gameObject.SetActive(true);
            PlayerCard playerCardUI = playerCard.GetComponent<PlayerCard>();

            playerCardUI.SetKickPlayerButtonVisible(
                LobbyManagerZK.Instance.IsLobbyHost() &&
                player.Id != AuthenticationService.Instance.PlayerId // Don't allow kick self
            );

            playerCardUI.UpdatePlayer(player);
        }

        lobbyNameText.text = lobby.Name;
        playerCountText.text = lobby.Players.Count + "/" + lobby.MaxPlayers;

        Show();
    }

    private void ClearLobby()
    {
        foreach (Transform child in container)
        {
            if (child == playerSingleTemplate) continue;
            Destroy(child.gameObject);
        }
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    void OnStartGamePressed()
    {
        if (!IsHost) return;

        // Optional: Only start if all players are ready
        if (!AreAllPlayersReady()) return;

        // Load game scene as host
        NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    void OnLeaveLobbyPressed()
    {
        LobbyManagerZK.Instance.LeaveLobby();
        SceneManager.LoadScene("MainMenu");
    }

    private bool AreAllPlayersReady()
    {
        var lobby = LobbyManagerZK.Instance.GetJoinedLobby();
        foreach (var player in lobby.Players)
        {
            if (!player.Data.ContainsKey("Ready") || player.Data["Ready"].Value != "true")
                return false;
        }
        return true;
    }
    
}
