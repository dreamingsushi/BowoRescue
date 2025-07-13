using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.Netcode;
public class BossFinishTrigger : NetworkBehaviour
{
    [SerializeField] private Boss boss;
    public float endSceneTimeDelay;
    public ParticleSystem vfx;
    public GameObject endSceneTimeline;
    public GameObject sadBowo;
    public GameObject endUI;
    public GameObject cage;

    private void OnTriggerEnter(Collider other)
    {
        if (!boss || !boss.isDead) return;

        if (!IsHost) return;

        if (other.CompareTag("Player")) // Make sure players are tagged correctly
        {
            Debug.Log("Boss defeated and player entered trigger!");
            TriggerEventClientRpc();
            StartCoroutine(TriggerEvent());
        }
    }

    [ClientRpc]
    private void TriggerEventClientRpc()
    {
        endSceneTimeline.SetActive(true);
        endUI.SetActive(true);
        StartCoroutine(DestroyCage());
    }

    // Host logic to time event, then load scene
    private IEnumerator TriggerEvent()
    {
        yield return new WaitForSeconds(endSceneTimeDelay + 1f); // Optional wait before scene load
        SendEveryoneToEndScene();
    }

    private IEnumerator DestroyCage()
    {
        yield return new WaitForSeconds(endSceneTimeDelay);
        vfx.Play();
        sadBowo.SetActive(false);
        cage.SetActive(false);
    }

    private void SendEveryoneToEndScene()
    {
        // Tell all clients (and host) to disconnect and load scene locally
        GoToEndSceneClientRpc();
        StartCoroutine(DisconnectAndLoadEndScene()); // Host does it too
    }

    [ClientRpc]
    private void GoToEndSceneClientRpc()
    {
        if (!IsHost)
        {
            StartCoroutine(DisconnectAndLoadEndScene());
        }
    }

    private IEnumerator DisconnectAndLoadEndScene()
    {
        NetworkManager.Singleton.Shutdown();
        yield return new WaitForSeconds(0.5f); // Wait a bit before loading
        SceneManager.LoadScene("EndScene");
    }
}
