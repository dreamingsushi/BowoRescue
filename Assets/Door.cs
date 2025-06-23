using UnityEngine;

public class Door : MonoBehaviour
{
    private Animator anim;
    private bool isOpened = false;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void TriggerDoor()
    {
        if (isOpened)
        {
            anim.SetTrigger("Close");
            isOpened = false;
        }
        else
        {
            anim.SetTrigger("Open");
            isOpened = true;
        }
    }
}
