using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float explosionDelay = 5f;
    public float damageAmount = 30f;
    public float explosionRadius = 5f;
    public GameObject explosionEffect;

    private void Start()
    {
        Invoke(nameof(Explode), explosionDelay);
    }

    private void Explode()
    {
        // Optional: spawn explosion visual effect
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Damage all nearby bosses (or enemies)
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out IDamageable target))
            {
                target.TakeDamage(damageAmount);
            }
        }

        // Destroy the bomb object
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
