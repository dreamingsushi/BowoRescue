using UnityEngine;

public class BossTriggerZone : MonoBehaviour
{
    public Boss bossScript; // Drag your boss GameObject here in Inspector
    public GameObject bossTimeline;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            bossScript.StartPhase1();
            bossTimeline.SetActive(true);
        }
    }

}
