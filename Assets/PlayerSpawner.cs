using UnityEngine;
using Unity.Services.Lobbies.Models;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;

    private void Start()
    {
        var lobby = MainMenuManager.Instance?.GetJoinedLobby();
        if (lobby == null) return;

        int index = 0;
        foreach (var player in lobby.Players)
        {
            if (player.Data.TryGetValue("PlayerName", out var name))
            {
                Vector3 spawnPos = spawnPoints[index % spawnPoints.Length].position;
                GameObject playerGO = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
                playerGO.name = name.Value;

                var playerLabel = playerGO.GetComponentInChildren<TMPro.TextMeshPro>();
                if (playerLabel) playerLabel.text = name.Value;

                index++;
            }
        }
    }
}
