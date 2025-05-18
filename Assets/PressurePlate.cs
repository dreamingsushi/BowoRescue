using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public Gate targetGate;
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
            targetGate.OpenGate();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody != null || other.CompareTag("Player"))
        {
            objectOnPlate = false;
            anim.SetBool("IsPressed", false);
            targetGate.CloseGate();
        }
    }
}
