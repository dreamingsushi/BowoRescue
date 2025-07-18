using UnityEngine;
using Unity.Netcode;

public class FlameDamage : MonoBehaviour
{
    public float damagePerSecond = 10f;

    private void OnTriggerStay(Collider other)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (other.CompareTag("Boss") || other.CompareTag("Enemy"))
            return;
        
        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(damagePerSecond * Time.deltaTime);
        }
    }
}
