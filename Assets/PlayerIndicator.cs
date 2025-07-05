using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerIndicator : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer indicatorRenderer; // Assign the circle Renderer in Inspector

    public NetworkVariable<int> playerIndex = new NetworkVariable<int>(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // Define your player colors (you can expand this)
    public Color[] playerColors = new Color[]
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow
    };

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Assign index from PlayerIndexManager on server
            int index = PlayerIndexManager.Instance.GetPlayerIndex(OwnerClientId);
            playerIndex.Value = index;
        }

        // Listen for index being set or changed
        playerIndex.OnValueChanged += OnPlayerIndexChanged;

        // In case index is already valid
        if (playerIndex.Value >= 0)
        {
            OnPlayerIndexChanged(-1, playerIndex.Value);
        }
    }

    private IEnumerator WaitAndSetColor()
    {
        // Wait until PlayerIndexManager is ready and returns a valid index
        int playerIndex = -1;

        yield return new WaitUntil(() =>
        {
            playerIndex = PlayerIndexManager.Instance.GetPlayerIndex(NetworkManager.Singleton.LocalClientId);
            return playerIndex >= 0;
        });

        if ((int)OwnerClientId < playerColors.Length)
        {
            SetIndicatorColor(playerColors[(int)OwnerClientId]);
        }
        else
        {
            Debug.LogWarning($"No color defined for player index {playerIndex}");
        }
    }

    private void OnPlayerIndexChanged(int oldValue, int newValue)
    {
        if (newValue >= 0 && newValue < playerColors.Length)
        {
            SetIndicatorColor(playerColors[newValue]);
        }
        else
        {
            Debug.LogWarning($"No color defined for player index {newValue}");
        }
    }

    private void SetIndicatorColor(Color color)
    {
        if (indicatorRenderer != null)
        {
            indicatorRenderer.color = color;
            Debug.Log("Set Player Color");
        }
        else
        {
            Debug.LogWarning("Indicator Renderer not assigned.");
        }
    }

}
