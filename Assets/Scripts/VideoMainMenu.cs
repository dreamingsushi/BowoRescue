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
        SceneTransitionManager.Instance.StartTransitionAndLoadScene(mainMenuSceneName);
    }
    public void OpenSkipMenu()
    {
        skipMenu.SetActive(true);
        vp.Pause();
        AudioManager.Instance.PlaySFX("Menu");
    }
    public void CloseSkipMenu()
    {
        skipMenu.SetActive(false);
        vp.Play();
        AudioManager.Instance.PlaySFX("Menu");
    }
    public void SkipButton()
    {
        AudioManager.Instance.PlaySFX("Menu");
        SceneTransitionManager.Instance.StartTransitionAndLoadScene(mainMenuSceneName);
    }
}
