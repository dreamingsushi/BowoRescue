using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;

public class LobbyUIManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform playerListContainer;
    public GameObject playerNamePrefab;

    private Lobby currentLobby;

    private void Start()
    {
        // Get lobby from MainMenuManager
        currentLobby = MainMenuManager.Instance?.GetJoinedLobby();

        if (currentLobby != null)
        {
            RefreshPlayerList();
            PollLobbyLoop();
        }
        else
        {
            Debug.LogWarning("LobbyUIManager: No lobby found!");
        }
    }

    private async void PollLobbyLoop()
    {
         while (true)
        {
            await Task.Delay(3000);

            if (currentLobby == null) return;

            try
            {
                currentLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
                MainMenuManager.Instance?.SetJoinedLobby(currentLobby);

                RefreshPlayerList();
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogWarning("Polling failed: " + ex.Message);
                return;
            }
        }
    }

    private void RefreshPlayerList()
    {
        // Clear old entries
        foreach (Transform child in playerListContainer)
        {
            Destroy(child.gameObject);
        }

        // Add new player entries
        foreach (var player in currentLobby.Players)
        {
            string playerName = "Unknown";

            if (player.Data != null && player.Data.TryGetValue("PlayerName", out var name))
            {
                playerName = name.Value;
            }

            GameObject entry = Instantiate(playerNamePrefab, playerListContainer);
            entry.GetComponent<TMP_Text>().text = playerName;
        }
    }
}
