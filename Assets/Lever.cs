using UnityEngine;

public class Lever : MonoBehaviour
{
    public Door door;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>(); // use your actual controller script name
            if (player != null)
            {
                player.SetNearbyLever(this);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.SetNearbyLever(null);
            }
        }
    }

    public void TriggerLever()
    {
        door.TriggerDoor();
    }
}
