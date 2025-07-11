using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class StatueManager : NetworkBehaviour
{
    public static StatueManager Instance;

    public GameObject bridge;
    public Collider colliderWall;
    public Statue[] statues;
    public GameObject portal;
    public float bridgeScaleDuration = 0.2f;
    private void Awake()
    {
        Instance = this;
    }
    public void CheckAllStatuesActivated()
    {
        foreach (var statue in statues)
        {
            if (!statue.IsActivated())
            {
                return;
            }
        }
        OnAllStatuesActivatedClientRpc();
    }


    [ClientRpc]
    private void OnAllStatuesActivatedClientRpc()
    {
        AudioManager.Instance.PlaySFX("BridgeAppear");
        bridge.SetActive(true);
        colliderWall.enabled = false;
        StartCoroutine(ScaleBridge());
        portal.SetActive(true);
    }

    private IEnumerator ScaleBridge()
    {
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;

        bridge.transform.localScale = startScale;
        float t = 0f;

        while (t < bridgeScaleDuration)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / bridgeScaleDuration);
            bridge.transform.localScale = Vector3.Lerp(startScale, endScale, progress);
            yield return null;
        }

        bridge.transform.localScale = endScale;
    }
}
