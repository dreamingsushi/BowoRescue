using TMPro;
using UnityEngine;

public class EmoteWheel : MonoBehaviour
{
    public GameObject emoteBubblePrefab; // Prefab with a TMP text inside
    public Transform spawnPoint;         // Where the emote appears (e.g. above player)

    public void Emote1() => SpawnEmoteBubble("Come Here!");
    public void Emote2() => SpawnEmoteBubble("Hello!");
    public void Emote3() => SpawnEmoteBubble("Careful!");
    public void Emote4() => SpawnEmoteBubble("???");

    void SpawnEmoteBubble(string message)
    {
        GameObject bubble = Instantiate(emoteBubblePrefab, spawnPoint);
        bubble.GetComponentInChildren<TextMeshProUGUI>().text = message;
        Destroy(bubble, 3f); // Destroy after 3 seconds
    }
}
