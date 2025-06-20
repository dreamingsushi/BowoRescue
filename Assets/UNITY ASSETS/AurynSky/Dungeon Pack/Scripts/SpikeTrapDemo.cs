using System.Collections;
using UnityEngine;

public class SpikeTrapDemo : MonoBehaviour
{
    public Animator spikeTrapAnim; // Animator for the SpikeTrap
    public float startDelay = 2f;
    public float openTime = 2f;
    public float closeTime = 2f;

    void Awake()
    {
        spikeTrapAnim = GetComponent<Animator>();
        StartCoroutine(StartTrap());
    }

    IEnumerator StartTrap()
    {
        yield return new WaitForSeconds(startDelay);
        StartCoroutine(TrapLoop());
    }

    IEnumerator TrapLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(openTime);
            spikeTrapAnim.SetTrigger("open");
            yield return new WaitForSeconds(closeTime);
            spikeTrapAnim.SetTrigger("close");

        }
    }
}
