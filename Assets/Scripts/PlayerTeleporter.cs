using UnityEngine;
using Unity.Netcode;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerTeleporter : NetworkBehaviour
{
    public Vector3 teleportDestination = new Vector3(0, 0, 0); // Set in inspector

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(TeleportAfterSceneLoad());
    }

    private IEnumerator TeleportAfterSceneLoad()
    {
        yield return new WaitUntil(() => IsSpawned && SceneManager.GetActiveScene().isLoaded);
        Teleport(teleportDestination);
    }

    public void Teleport(Vector3 destination)
    {
        if (IsServer)
        {
            PerformTeleport(destination);
            TeleportClientRpc(destination); // Update all clients
        }
        else if (IsOwner)
        {
            TeleportServerRpc(destination); // Ask server to handle it
        }
    }
    private void PerformTeleport(Vector3 destination)
    {
        var controller = GetComponent<CharacterController>();
        if (controller) controller.enabled = false;

        transform.position = destination;

        if (controller) controller.enabled = true;

        Debug.Log($"Teleported {OwnerClientId} to {destination}");
    }

    [ServerRpc]
    private void TeleportServerRpc(Vector3 destination)
    {
        PerformTeleport(destination);
        TeleportClientRpc(destination);
    }

    [ClientRpc]
    private void TeleportClientRpc(Vector3 destination)
    {
        PerformTeleport(destination);
    }
    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            Teleport(teleportDestination);
            var playerController = GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = true;
                playerController.EnableInputsClientRpc();
            }
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Level 3 (boss)", LoadSceneMode.Single);
        }
        
        if (Input.GetKeyDown(KeyCode.P))
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Level 2", LoadSceneMode.Single);
        }
    }
}
