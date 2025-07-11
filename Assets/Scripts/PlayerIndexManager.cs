using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerIndexManager : NetworkBehaviour
{
    public static PlayerIndexManager Instance;

    private List<ulong> connectedClientIds = new List<ulong>();

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
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!connectedClientIds.Contains(clientId))
        {
            connectedClientIds.Add(clientId);
            Debug.Log($"Client {clientId} connected. Assigned index {connectedClientIds.IndexOf(clientId)}");
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        connectedClientIds.Remove(clientId);
    }

    public int GetPlayerIndex(ulong clientId)
    {
        return connectedClientIds.IndexOf(clientId); // returns 0, 1, 2, 3...
    }
}
