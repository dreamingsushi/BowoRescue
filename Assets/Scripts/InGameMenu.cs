using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class InGameMenu : MonoBehaviour
{
    public void ExitToMainMenu()
    {
        // Disconnect from NGO networking
        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                // Host shuts down everything
                NetworkManager.Singleton.Shutdown();
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                // Client disconnects
                NetworkManager.Singleton.Shutdown();
            }
        }

        // Leave the lobby (custom manager, safe call)
        if (LobbyManagerZK.Instance != null)
        {
            LobbyManagerZK.Instance.LeaveLobby();
        }

        // Load main menu
        SceneManager.LoadScene("MainMenu");
    }

    public void BackToGame()
    {
        gameObject.SetActive(false);
    }
}
