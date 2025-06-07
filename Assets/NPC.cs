using UnityEngine;

public class NPC : MonoBehaviour
{
    public State currentState = State.Idle;
    public DialogueData dialogueData;
    private Canvas msgIcon;
    private Quaternion originalRotation;
    private Transform player;

    public enum State
    {
        Idle, Talking
    }
    void Start()
    {
        msgIcon = GetComponentInChildren<Canvas>();
        msgIcon.gameObject.SetActive(false);

        originalRotation = transform.rotation;
    }
    void Update()
    {
        if (currentState == State.Talking && player != null)
        {
            // Look at player but only rotate on Y axis
            Vector3 direction = player.position - transform.position;
            direction.y = 0f;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }
        else if (currentState == State.Idle)
        {
            // Smoothly return to original Y rotation
            Quaternion targetRotation = Quaternion.Euler(0f, originalRotation.eulerAngles.y, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            msgIcon.gameObject.SetActive(true);
            player = other.transform;
            other.GetComponent<PlayerController>()?.SetNearbyNPC(this);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            msgIcon.gameObject.SetActive(false);
            currentState = State.Idle;
            player = null;
            other.GetComponent<PlayerController>()?.ClearNearbyNPC();
        }
    }

    public void StartTalking()
    {
        currentState = State.Talking;
        // Rotate to player
    }

    public void StopTalking()
    {
        currentState = State.Idle;
        // Reset rotation
    }

}
