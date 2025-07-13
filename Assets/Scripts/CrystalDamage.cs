using UnityEngine;

public class CrystalDamage : MonoBehaviour
{
    public float burstDamage = 25f;
    public float slowMultiplier = 0.5f;
    public float slowDuration = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boss") || other.CompareTag("Enemy"))
            return;

        // Apply burst damage
        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(burstDamage);
        }

        // Apply slow effect
        if (other.TryGetComponent(out PlayerController player))
        {
            player.SetSlow(slowMultiplier, slowDuration);
        }
    }
}
