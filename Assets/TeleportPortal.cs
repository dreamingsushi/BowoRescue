using UnityEngine;
using System.Collections;

public class TeleportPortal : MonoBehaviour
{
    [Header("Destination")]
    public TeleportPortal destinationPortal;
    public Collider destinationPortalCollider;
    public PlayerController playerController;

    private bool isTeleporting = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!isTeleporting && other.CompareTag("Player"))
        {
            Debug.Log("Teleporting");
            StartCoroutine(Teleport(other.transform));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Reset teleporting flag only when the player exits
        if (other.CompareTag("Player"))
        {
            isTeleporting = false;
            Debug.Log("Player exited portal");
        }
    }
    

    private IEnumerator Teleport(Transform player)
    {
        isTeleporting = true;
        playerController.enabled = false;
        Vector3 originalScale = player.localScale;

        // Step 1: Sink into current portal
        yield return StartCoroutine(SinkIntoPortal(player, 0.25f, originalScale));

        // Step 2: Move player to destination
        player.position = destinationPortal.transform.position;

        // Step 3: Temporarily disable destination portal collider
        destinationPortalCollider.enabled = false;

        // Step 4: Pop out of destination portal
        yield return destinationPortal.StartCoroutine(destinationPortal.PopOutOfPortal(player, 0.25f, originalScale));

        // Step 5: Wait until player exits destination portal, then re-enable it
        yield return new WaitUntil(() => !destinationPortalCollider.bounds.Contains(player.position));
        destinationPortalCollider.enabled = true;

        yield return new WaitForSeconds(0.25f);
        playerController.enabled = true;
        isTeleporting = false;
    }


    IEnumerator SinkIntoPortal(Transform player, float duration, Vector3 originalScale)
    {
        Vector3 endScale = new Vector3(originalScale.x, 0.1f, originalScale.z);

        Vector3 startPos = player.position;
        Vector3 endPos = startPos + Vector3.down * 0.5f; // move slightly down

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
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
        Vector3 startScale = new Vector3(originalScale.x, 0.1f, originalScale.z); // almost flat
        Vector3 endScale = originalScale;

        Vector3 startPos = player.position + Vector3.down * 0.5f;
        Vector3 endPos = player.position;

        player.localScale = startScale;
        player.position = startPos;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            player.localScale = Vector3.Lerp(startScale, endScale, t);
            player.position = Vector3.Lerp(startPos, endPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        player.localScale = endScale;
        player.position = endPos;
    }


}
