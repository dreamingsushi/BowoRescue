using UnityEngine;
using System;
using Unity.Netcode;
using UnityEngine.SceneManagement;
public class MainMenuManager : NetworkBehaviour
{

    private string lobbyName;
    private bool isPrivate;
    private int maxPlayers;
    public void OnStartButtonClicked()
    {

    }

    public void OnHostButtonClicked()
    {
        LobbyManagerZK.Instance.OnJoinedLobby += HandleLobbyJoinedHost;
        LobbyManagerZK.Instance.CreateLobby();
    }

    private void HandleLobbyJoinedHost(object sender, EventArgs e)
    {
        LobbyManagerZK.Instance.OnJoinedLobby -= HandleLobbyJoinedHost;

        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("LobbyScene", LoadSceneMode.Single);
    }

    public void OnJoinButtonClicked()
    {
        LobbyManagerZK.Instance.OnJoinedLobby += HandleLobbyJoinedClient;
        LobbyManagerZK.Instance.QuickJoinLobby();
    }

    private void HandleLobbyJoinedClient(object sender, EventArgs e)
    {
        LobbyManagerZK.Instance.OnJoinedLobby -= HandleLobbyJoinedClient;

        NetworkManager.Singleton.StartClient();
    }



}
