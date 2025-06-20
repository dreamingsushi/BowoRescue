using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class Portal : NetworkBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Level 2", LoadSceneMode.Single);
        }
    }
}
