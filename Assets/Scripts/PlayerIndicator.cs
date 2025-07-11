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
        StartCoroutine(WaitAndSetColor());
    }

    private IEnumerator WaitAndSetColor()
    {
        yield return new WaitForSeconds(1f);

        if ((int)OwnerClientId < playerColors.Length)
        {
            SetIndicatorColor(playerColors[(int)OwnerClientId]);
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
