using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class InGameMenu : MonoBehaviour
{
    public GameObject settingsPanel;
    public GameObject gameMenuUI;
    public void ExitToMainMenu()
    {
        AudioManager.Instance.PlaySFX("Menu");
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
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.LeaveLobby();
        }

        // Load main menu
        SceneManager.LoadScene("MainMenu");
    }

    public void BackToGame()
    {
        gameObject.SetActive(false);
        AudioManager.Instance.PlaySFX("Menu");
    }

    public void Settings()
    {
        settingsPanel.SetActive(true);
        gameMenuUI.SetActive(false);
        AudioManager.Instance.PlaySFX("Menu");
    }

    public void BackToGameMenu()
    {
        settingsPanel.SetActive(false);
        gameMenuUI.SetActive(true);
        AudioManager.Instance.PlaySFX("Menu");        
    }

    public void SetMasterVolume(float volume)
    {
        AudioManager.Instance.SetMasterVolume(volume);
    }
    public void SetSFXVolume(float volume)
    {
        AudioManager.Instance.SetSFXVolume(volume);
    }
    public void SetMusicVolume(float volume)
    {
        AudioManager.Instance.SetMusicVolume(volume);
    }
}
