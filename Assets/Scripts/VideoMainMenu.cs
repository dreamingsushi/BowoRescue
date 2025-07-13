using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoToMainMenu : MonoBehaviour
{
    public string mainMenuSceneName = "MainMenu"; // Change to your scene name

    void Start()
    {
        VideoPlayer vp = GetComponent<VideoPlayer>();
        if (vp != null)
        {
            vp.loopPointReached += OnVideoEnd;
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void SkipButton()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
