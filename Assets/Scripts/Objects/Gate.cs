using UnityEngine;

public class Gate : MonoBehaviour
{
    private Animator anim;
    [SerializeField] private AudioSource audioSource;

    void Start()
    {
        anim = GetComponent<Animator>();
    }
    public void OpenGate()
    {
        anim.SetTrigger("OpenGate");
        audioSource.PlayOneShot(audioSource.clip);
    }

    public void CloseGate()
    {
        anim.SetTrigger("CloseGate");
        audioSource.PlayOneShot(audioSource.clip);
    }
}
