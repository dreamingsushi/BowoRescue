using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoToMainMenu : MonoBehaviour
{
    public string mainMenuSceneName = "MainMenu"; // Change to your scene name
    public GameObject skipMenu;
    public VideoPlayer vp;
    void Start()
    {
        vp = GetComponent<VideoPlayer>();
        if (vp != null)
        {
            vp.loopPointReached += OnVideoEnd;
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
    public void OpenSkipMenu()
    {
        skipMenu.SetActive(true);
        vp.Pause();
    }
    public void CloseSkipMenu()
    {
        skipMenu.SetActive(false);
        vp.Play();
    }
    public void SkipButton()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
