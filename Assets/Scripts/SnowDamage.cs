using UnityEngine;
using Unity.Netcode;

public class SnowDamage : MonoBehaviour
{
    public float damagePerSecond = 10f;
    public float slowMultiplier = 0.5f;
    public float slowDuration = 1f;

    private void OnTriggerStay(Collider other)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (other.CompareTag("Boss") || other.CompareTag("Enemy"))
            return;
        
        // Damage
        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(damagePerSecond * Time.deltaTime);
        }

        // Slow
        if (other.TryGetComponent(out PlayerController player))
        {
            player.SetSlow(slowMultiplier, slowDuration);
        }
    }
}
