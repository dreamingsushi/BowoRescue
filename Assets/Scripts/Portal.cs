using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class Portal : NetworkBehaviour
{
    public bool level1;
    public bool level2;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (level1)
            {
                //NetworkManager.Singleton.SceneManager.LoadScene("Level 2", LoadSceneMode.Single);
                SceneTransitionManager.Instance.StartTransitionAndLoadScene("Level 2");
            }
            else if (level2)
            {
                //NetworkManager.Singleton.SceneManager.LoadScene("Level 3 (boss)", LoadSceneMode.Single);
                SceneTransitionManager.Instance.StartTransitionAndLoadScene("Level 3 (boss)");
            }
        }
    }
}
