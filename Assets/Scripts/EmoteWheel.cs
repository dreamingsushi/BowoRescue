using Unity.Netcode;
using UnityEngine;
using TMPro;

public class EmoteWheel : NetworkBehaviour
{
    [Header("Emote UI")]
    public GameObject emoteBubblePrefab;  // UI prefab (NO NetworkObject!)
    public Transform spawnPoint;          // World Space canvas location (e.g. above head)

    // Called by local player input (via button or key)
    public void Emote1() => SendEmoteServerRpc("Come Here!");
    public void Emote2() => SendEmoteServerRpc("Hello!");
    public void Emote3() => SendEmoteServerRpc("Careful!");
    public void Emote4() => SendEmoteServerRpc("???");

    // Tell server: I want to send this emote
    [ServerRpc(RequireOwnership = false)]
    void SendEmoteServerRpc(string message, ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;

        // Now tell everyone (including sender) to show this emote on the right player
        SendEmoteClientRpc(message, senderClientId);
    }

[ClientRpc]
void SendEmoteClientRpc(string message, ulong senderClientId)
{
    foreach (var player in NetworkManager.Singleton.ConnectedClientsList)
    {
        var playerObject = player.PlayerObject;

        if (playerObject != null && playerObject.OwnerClientId == senderClientId)
        {
            EmoteWheel wheel = playerObject.GetComponentInChildren<EmoteWheel>(true); // include inactive
            if (wheel != null)
            {
                wheel.SpawnEmoteBubble(message);
                return;
            }
        }
    }

    Debug.LogWarning("No player/EmoteWheel found for client ID " + senderClientId);
}


    // Spawns a local emote bubble (TMP text) under the world canvas
    public void SpawnEmoteBubble(string message)
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning("spawnPoint not assigned on " + gameObject.name);
            return;
        }

        GameObject bubble = Instantiate(emoteBubblePrefab, spawnPoint);
        var text = bubble.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
            text.text = message;

        Destroy(bubble, 3f); // Auto-destroy after 3 seconds
    }
}
