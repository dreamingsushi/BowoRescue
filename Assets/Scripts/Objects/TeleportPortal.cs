using UnityEngine;
using System.Collections;
using Unity.Netcode;
using System.Collections.Generic;

public class TeleportPortal : NetworkBehaviour
{
    [Header("Destination")]
    public TeleportPortal destinationPortal;
    public Collider destinationPortalCollider;

    [SerializeField] private Vector3 originalScale;

    // Tracks teleporting players individually
    private HashSet<ulong> teleportingClients = new HashSet<ulong>();

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (!other.CompareTag("Player")) return;

        NetworkObject netObj = other.GetComponent<NetworkObject>();
        if (netObj == null || teleportingClients.Contains(netObj.OwnerClientId)) return;

        PlayerController playerController = other.GetComponent<PlayerController>();
        if (playerController == null) return;

        Debug.Log("Server: Teleporting player " + netObj.OwnerClientId);
        teleportingClients.Add(netObj.OwnerClientId);
        StartCoroutine(Teleport(netObj, playerController));
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer || !other.CompareTag("Player")) return;

        NetworkObject netObj = other.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            teleportingClients.Remove(netObj.OwnerClientId);
            Debug.Log("Server: Player exited portal " + netObj.OwnerClientId);
        }
    }

    private IEnumerator Teleport(NetworkObject playerNetObj, PlayerController playerController)
    {
        // Disable player control
        playerController.DisableInputs();
        playerController.enabled = false;

        Transform player = playerController.transform;
        Vector3 playerOriginalScale = player.localScale;

        // Step 1: Sink
        yield return StartCoroutine(SinkIntoPortal(player, 0.25f, playerOriginalScale));

        // Step 2: Move to destination
        AudioManager.Instance.PlaySFX("Teleport");
        player.position = destinationPortal.transform.position + new Vector3(0, 1.2f, 0);

        // Step 3: Destination logic
        destinationPortal.teleportingClients.Add(playerNetObj.OwnerClientId);
        yield return destinationPortal.StartCoroutine(destinationPortal.PopOutOfPortal(player, 0.25f, originalScale));
        yield return new WaitForSeconds(0.02f);

        destinationPortal.teleportingClients.Remove(playerNetObj.OwnerClientId);

        // Step 4: Re-enable controls
        playerController.EnableInputs();
        playerController.enabled = true;
        playerController.Jump();
    }

    IEnumerator SinkIntoPortal(Transform player, float duration, Vector3 originalScale)
    {
        Vector3 endScale = new Vector3(originalScale.x, 0.1f, originalScale.z);
        Vector3 startPos = player.position;
        Vector3 endPos = startPos + Vector3.down * 0.5f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            player.localScale = Vector3.Lerp(originalScale, endScale, t);
            player.position = Vector3.Lerp(startPos, endPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        player.localScale = endScale;
        player.position = endPos;
    }

    IEnumerator PopOutOfPortal(Transform player, float duration, Vector3 originalScale)
    {
        Vector3 startScale = new Vector3(originalScale.x, 0.1f, originalScale.z);
        Vector3 endScale = originalScale;

        Vector3 startPos = player.position + Vector3.down * 0.5f;
        Vector3 endPos = player.position;

        player.localScale = startScale;
        player.position = startPos;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            player.localScale = Vector3.Lerp(startScale, endScale, t);
            player.position = Vector3.Lerp(startPos, endPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        player.localScale = endScale;
        player.position = endPos;
    }
}
