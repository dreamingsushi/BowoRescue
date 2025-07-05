using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerIndicator : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer indicatorRenderer;

    public NetworkVariable<int> playerIndex = new NetworkVariable<int>(
    -1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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
            int index = PlayerIndexManager.Instance.GetPlayerIndex(OwnerClientId);
            playerIndex.Value = index;
            Debug.Log($"[SERVER] Assigned index {index} to client {OwnerClientId}");
        }

        StartCoroutine(DelayedSetColor());
    }

    private IEnumerator DelayedSetColor()
    {
        yield return new WaitUntil(() => playerIndex.Value >= 0);

        if (playerIndex.Value < playerColors.Length)
        {
            SetIndicatorColor(playerColors[playerIndex.Value]);
        }
        else
        {
            Debug.LogWarning($"No color defined for player index {playerIndex.Value}");
        }
    }

    private void SetIndicatorColor(Color color)
    {
        if (indicatorRenderer != null)
        {
            indicatorRenderer.color = color;
        }
        else
        {
            Debug.LogWarning("Indicator Renderer not assigned.");
        }
    }
}
