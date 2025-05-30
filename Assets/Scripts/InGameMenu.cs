using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameMenu : MonoBehaviour
{
    public void ExitToMainMenu()
    {
        LobbyManagerZK.Instance.LeaveLobby();
        SceneManager.LoadScene("MainMenu");
    }
}
