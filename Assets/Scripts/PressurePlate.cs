using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public Gate targetGate;
    private Animator anim;
    private Renderer buttonRenderer; // Assign the button’s mesh renderer here
    private bool objectOnPlate = false;
    private Material buttonMaterial;

    public Color defaultColor = Color.red;
    public Color pressedColor = Color.green;

    void Start()
    {
        anim = GetComponent<Animator>();
        buttonRenderer = GetComponent<Renderer>();

        if (buttonRenderer != null)
        {
            buttonMaterial = buttonRenderer.material;
            buttonMaterial.color = defaultColor;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody != null || other.CompareTag("Player"))
        {
            objectOnPlate = true;
            anim.SetBool("IsPressed", true);
            targetGate.OpenGate();

            if (buttonMaterial != null)
                buttonMaterial.color = pressedColor;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody != null || other.CompareTag("Player"))
        {
            objectOnPlate = false;
            anim.SetBool("IsPressed", false);
            targetGate.CloseGate();

            if (buttonMaterial != null)
                buttonMaterial.color = defaultColor;
        }
    }
}
