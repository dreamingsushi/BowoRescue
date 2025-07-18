using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Collections;

public class SceneTransitionManager : NetworkBehaviour
{
    public static SceneTransitionManager Instance;
    [SerializeField] private Animator anim;
    [SerializeField] private float transitionTime = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(transform.root.gameObject);
        }

        if (SceneManager.GetActiveScene().name == "StartScene") return;

        EndTransition();
    }
    public void StartTransitionAndLoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneWithTransition(sceneName));
    }
    public void StartTransitionAndLoadLocalScene(string sceneName)
    {
        StartCoroutine(LoadLocalSceneWithTransition(sceneName));
    }

    private IEnumerator LoadSceneWithTransition(string sceneName)
    {
        anim.SetTrigger("Start");
        PlayTransitionClientRpc();

        yield return new WaitForSeconds(transitionTime);

        // Use NetworkSceneManager if using Netcode for GameObjects
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
        yield return new WaitUntil(() => SceneManager.GetActiveScene().isLoaded);
        yield return new WaitForSeconds(1);
        anim.SetTrigger("End");
        EndTransitionClientRpc();
    }

    private IEnumerator LoadLocalSceneWithTransition(string sceneName)
    {
        anim.SetTrigger("Start");
        PlayTransitionClientRpc();

        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(sceneName);
        yield return new WaitUntil(() => SceneManager.GetActiveScene().isLoaded);
        yield return new WaitForSeconds(1);
        anim.SetTrigger("End");
    }

    // Optional: call this on scene load complete to fade out
    public void EndTransition()
    {
        anim.SetTrigger("End");
    }

    [ClientRpc]
    void PlayTransitionClientRpc()
    {
        if (anim != null)
            anim.SetTrigger("Start");
    }

    [ClientRpc]
    void EndTransitionClientRpc()
    {
        if (anim != null)
            anim.SetTrigger("End");
    }

}
