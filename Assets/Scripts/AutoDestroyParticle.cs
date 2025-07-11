using UnityEngine;

public class AutoDestroyParticle : MonoBehaviour
{
    void Start()
    {
        Destroy(gameObject, GetComponent<ParticleSystem>().main.duration + 0.1f);
    }
}

