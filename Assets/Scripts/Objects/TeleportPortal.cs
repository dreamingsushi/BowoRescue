using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TeleportPortal : MonoBehaviour
{
    [Header("Destination")]
    public TeleportPortal destinationPortal;
    public Collider destinationPortalCollider;
    private PlayerController playerController;
    [SerializeField] private Vector3 originalScale;

    private bool isTeleporting = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!isTeleporting && other.CompareTag("Player"))
        {
            playerController = other.gameObject.GetComponent<PlayerController>();
            if (playerController == null) return;
            Debug.Log("Teleporting");
            StartCoroutine(Teleport(other.transform));
        }
    }

    private void OnTriggerExit(Collider other)
    {
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
        playerController.DisableInputs();
        Vector3 playerOriginalScale = player.localScale;

        // Step 1: Sink into current portal
        yield return StartCoroutine(SinkIntoPortal(player, 0.25f, playerOriginalScale));

        // Step 2: Move player to destination
        AudioManager.Instance.PlaySFX("Teleport");
        player.position = destinationPortal.transform.position + new Vector3(0, 1.2f, 0);

        // Step 3: Temporarily disable destination portal collider
        destinationPortal.isTeleporting = true;

        // Step 4: Pop out of destination portal
        yield return destinationPortal.StartCoroutine(destinationPortal.PopOutOfPortal(player, 0.25f, originalScale));

        // Step 5: Wait until player exits destination portal, then re-enable it
        yield return new WaitForSeconds(0.02f);
        //yield return new WaitUntil(() => !destinationPortalCollider.bounds.Contains(player.position));
        destinationPortal.isTeleporting = false;

        playerController.EnableInputs();
        playerController.enabled = true;
        playerController.Jump();

        isTeleporting = false;
        playerController = null;
    }


    IEnumerator SinkIntoPortal(Transform player, float duration, Vector3 originalScale)
    {
        Vector3 endScale = new Vector3(originalScale.x, 0.1f, originalScale.z);

        Vector3 startPos = player.position;
        Vector3 endPos = startPos + Vector3.down * 0.5f; // move slightly down

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
        Vector3 startScale = new Vector3(originalScale.x, 0.1f, originalScale.z); // almost flat
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
