using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public GameObject door;
    public Animator anim;
    private bool objectOnPlate = false;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody != null || other.CompareTag("Player"))
        {
            objectOnPlate = true;
            anim.SetBool("IsPressed", true);
            OpenDoor();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody != null || other.CompareTag("Player"))
        {
            objectOnPlate = false;
            anim.SetBool("IsPressed", false);
            CloseDoor();
        }
    }

    void OpenDoor()
    {
        if (door != null)
            door.SetActive(false); // Or use animator if needed
        Debug.Log("Door opened!");
    }

    void CloseDoor()
    {
        if (door != null)
            door.SetActive(true); // Or use animator if needed
        Debug.Log("Door closed!");
    }
}
