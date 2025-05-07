using UnityEngine;
using System.Collections;

public class TeleportPortal : MonoBehaviour
{
    [Header("Destination")]
    public TeleportPortal destinationPortal;

    private bool isTeleporting = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!isTeleporting && other.gameObject.CompareTag("Player"))
        {
            StartCoroutine(Teleport(other.transform));
        }
    }

    private IEnumerator Teleport(Transform player)
    {
        isTeleporting = true;

        // Step 1: Sink into current portal
        yield return StartCoroutine(SinkIntoPortal(player, 0.25f));

        // Step 2: Move player to destination
        player.position = destinationPortal.transform.position;

        // Step 3: Pop out of destination portal
        yield return destinationPortal.StartCoroutine(destinationPortal.PopOutOfPortal(player, 0.25f));

        yield return new WaitForSeconds(0.25f);
        isTeleporting = false;
    }


    IEnumerator SinkIntoPortal(Transform player, float duration)
    {
        Vector3 startScale = player.localScale;
        Vector3 endScale = new Vector3(startScale.x, 0.1f, startScale.z); // flatten vertically

        Vector3 startPos = player.position;
        Vector3 endPos = startPos + Vector3.down * 0.5f; // move slightly down

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

    IEnumerator PopOutOfPortal(Transform player, float duration)
    {
        Vector3 startScale = new Vector3(player.localScale.x, 0.1f, player.localScale.z); // almost flat
        Vector3 endScale = new Vector3(1f, 1f, 1f);

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
