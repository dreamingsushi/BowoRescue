using UnityEngine;

public class Gate : MonoBehaviour
{
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }
    public void OpenGate()
    {
        anim.SetTrigger("OpenGate");
    }

    public void CloseGate()
    {
        anim.SetTrigger("CloseGate");
    }
}
