using UnityEngine;
using System;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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
        LobbyManagerZK.Instance.OnJoinedLobby += HandleLobbyJoinedHost;
        LobbyManagerZK.Instance.CreateLobby();
        AudioManager.Instance.PlaySFX("Menu");
    }

    private void HandleLobbyJoinedHost(object sender, EventArgs e)
    {
        LobbyManagerZK.Instance.OnJoinedLobby -= HandleLobbyJoinedHost;

        LobbyManagerZK.Instance.CreateRelayAndStartHost();
        //NetworkManager.Singleton.StartHost();
        //NetworkManager.Singleton.SceneManager.LoadScene("LobbyScene", LoadSceneMode.Single);
    }

    public void OnJoinButtonClicked()
    {
        SceneTransitionManager.Instance.StartTransitionAndLoadScene("LobbyScene");
        LobbyManagerZK.Instance.OnJoinedLobby += HandleLobbyJoinedClient;
        LobbyManagerZK.Instance.QuickJoinLobby();
        AudioManager.Instance.PlaySFX("Menu");
    }

    private void HandleLobbyJoinedClient(object sender, EventArgs e)
    {
        LobbyManagerZK.Instance.OnJoinedLobby -= HandleLobbyJoinedClient;
        LobbyManagerZK.Instance.JoinRelayAndStartClient();
        //NetworkManager.Singleton.StartClient();
    }

    public void ExitGame()
    {
        Application.Quit();
    }



}
