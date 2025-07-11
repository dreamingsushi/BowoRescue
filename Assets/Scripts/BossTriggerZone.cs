using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.Playables;

public class BossTriggerZone : MonoBehaviour
{
    public Boss bossScript; // Drag your boss GameObject here in Inspector
    public GameObject bossTimeline;
    public List<Transform> bossArenaSpawnPoints;
    public PlayableDirector bossDirector;
    public GameObject bossHealthBar;
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return;
        hasTriggered = true;
        TeleportAllPlayers();

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject.TryGetComponent(out PlayerController controller))
            {
                controller.DisableInputs();
            }
        }

        bossDirector.stopped += OnTimelineFinished;
        bossTimeline.SetActive(true);
        bossDirector.Play();
    }

    private void TeleportAllPlayers()
    {
        var clients = NetworkManager.Singleton.ConnectedClientsList;
        for (int i = 0; i < clients.Count; i++)
        {
            var playerObj = clients[i].PlayerObject;
            if (playerObj.TryGetComponent(out PlayerTeleporter player))
            {
                var spawnPoint = bossArenaSpawnPoints[Mathf.Min(i, bossArenaSpawnPoints.Count - 1)];
                player.Teleport(spawnPoint.position);;
            }
        }
    }

    private void OnTimelineFinished(PlayableDirector director)
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject.TryGetComponent(out PlayerController controller))
            {
                controller.EnableInputs();
                bossScript.StartPhase1();
                AudioManager.Instance.PlayMusic("BossTheme");
                bossHealthBar.SetActive(true);
            }
        }

        bossDirector.stopped -= OnTimelineFinished; // Cleanup
    }


}
