using UnityEngine;

public class BossFInishTrigger : MonoBehaviour
{
    [SerializeField] private Boss boss; // Drag the Boss object with the Boss script in Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (!boss || !boss.isDead) return;

        if (other.CompareTag("Player")) // Make sure players are tagged correctly
        {
            Debug.Log("Boss defeated and player entered trigger!");
            TriggerEvent();
        }
    }

    private void TriggerEvent()
    {
        
    }
}
