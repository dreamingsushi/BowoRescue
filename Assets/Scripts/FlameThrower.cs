using UnityEngine;
using System.Collections;

public class FlameThrower : MonoBehaviour
{
    public ParticleSystem fireVfx;
    public Animator anim;
    public float fireDuration = 2f;  // How long flame is active
    public float pauseDuration = 2f; // How long between flames

    private Coroutine fireRoutine;

    void Start()
    {
        fireRoutine = StartCoroutine(FireSequence());
    }

    IEnumerator FireSequence()
    {
        while (true)
        {
            anim.SetTrigger("OpenFire");
            fireVfx.Play();
            yield return new WaitForSeconds(fireDuration);

            anim.SetTrigger("CloseFire");
            fireVfx.Stop();
            yield return new WaitForSeconds(pauseDuration);
        }
    }

    // Optional manual stop
    public void ForceStopFire()
    {
        if (fireRoutine != null)
        {
            StopCoroutine(fireRoutine);
            fireRoutine = null;

            anim.SetTrigger("CloseFire");
            fireVfx.Stop();
        }
    }
}
