using UnityEngine;
using System;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.VisualScripting;
public class MainMenuManager : NetworkBehaviour
{

    private string lobbyName;
    private bool isPrivate;
    private int maxPlayers;
    public Button targetButton;
    public void OnStartButtonClicked()
    {

    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) return;
            // Optional: Check if the button is interactable and visible
            if (targetButton != null && targetButton.interactable && targetButton.gameObject.activeInHierarchy)
            {
                targetButton.onClick.Invoke();
            }
        }
    }

    public void OnHostButtonClicked()
    {
        SceneTransitionManager.Instance.StartTransitionAndLoadScene("LobbyScene");
        LobbyManager.Instance.OnJoinedLobby += HandleLobbyJoinedHost;
        LobbyManager.Instance.CreateLobby();
        AudioManager.Instance.PlaySFX("Menu");
    }

    private void HandleLobbyJoinedHost(object sender, EventArgs e)
    {
        LobbyManager.Instance.OnJoinedLobby -= HandleLobbyJoinedHost;
        LobbyManager.Instance.CreateRelayAndStartHost();
    }

    public void OnJoinButtonClicked()
    {
        SceneTransitionManager.Instance.StartTransitionAndLoadScene("LobbyScene");
        LobbyManager.Instance.OnJoinedLobby += HandleLobbyJoinedClient;
        LobbyManager.Instance.QuickJoinLobby();
        AudioManager.Instance.PlaySFX("Menu");
    }

    private void HandleLobbyJoinedClient(object sender, EventArgs e)
    {
        LobbyManager.Instance.OnJoinedLobby -= HandleLobbyJoinedClient;
        LobbyManager.Instance.JoinRelayAndStartClient();
    }

    public void ExitGame()
    {
        Application.Quit();
    }



}
