using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerIndicator : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer indicatorRenderer; // Assign the circle Renderer in Inspector

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
        if (IsOwner)
        {
            StartCoroutine(WaitAndSetColor());
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

        if (playerIndex < playerColors.Length)
        {
            SetIndicatorColor(playerColors[playerIndex]);
        }
        else
        {
            Debug.LogWarning($"No color defined for player index {playerIndex}");
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
