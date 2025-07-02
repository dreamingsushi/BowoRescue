using UnityEngine;

public class Lever : MonoBehaviour
{
    public Door door;
    private Animator anim;
    private bool opened;
    [SerializeField] private GameObject highlightEffect;
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>(); // use your actual controller script name
            if (player != null)
            {
                player.SetNearbyLever(this);
            }
            highlightEffect.SetActive(true);
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
            highlightEffect.SetActive(false);
        }
    }

    public void TriggerLever()
    {
        door.TriggerDoor();

        if (opened)
        {
            anim.SetTrigger("Close");
        }
        else
        {
            anim.SetTrigger("Open");
        }
    }
}
