using UnityEngine;
using Unity.Netcode;
public class PlayerSpawner : NetworkBehaviour
{
    // [SerializeField] private GameObject playerPrefab;
    // [SerializeField] private Transform spawnPoint;

    // private void OnEnable()
    // {
    //     NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    // }

    // private void OnDisable()
    // {
    //     NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    // }

    // private void OnClientConnected(ulong clientId)
    // {
    //     if (!NetworkManager.Singleton.IsServer) return;

    //     SpawnPlayerForClient(clientId);
    // }

    // private void SpawnPlayerForClient(ulong clientId)
    // {
    //     var playerInstance = Instantiate(playerPrefab);
    //     playerInstance.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
    // }

}
