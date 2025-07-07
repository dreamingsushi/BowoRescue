using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;
    [SerializeField] private Animator anim;
    [SerializeField] private float transitionTime = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (SceneManager.GetActiveScene().name == "MainMenu") return;

        EndTransition();
    }
    public void StartTransitionAndLoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneWithTransition(sceneName));
    }

    private IEnumerator LoadSceneWithTransition(string sceneName)
    {
        anim.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        // Use NetworkSceneManager if using Netcode for GameObjects
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        yield return new WaitUntil(() => SceneManager.GetActiveScene().isLoaded);
        
        anim.SetTrigger("End");
    }

    // Optional: call this on scene load complete to fade out
    public void EndTransition()
    {
        anim.SetTrigger("End");
    }
}
