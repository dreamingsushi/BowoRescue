using Unity.Netcode;
using UnityEngine;

public class PlayerDamageCollider : MonoBehaviour
{
    public float damageAmount;
    public float knockbackStrength = 20f;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            return;

        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(damageAmount);
            Debug.Log("Attacked" + damageable + "For" + damageAmount);
            HitStopManager.Instance?.DoHitStop(0.02f);
            CameraShakeManager.Instance?.Shake();
        }

        if (other.TryGetComponent(out IKnockbackable knockbackable))
        {
            Vector3 direction = (other.transform.position - transform.position).normalized;
            Vector3 force = direction * knockbackStrength;
            knockbackable.GetKnockedBack(force);
        }

        if (other.TryGetComponent<NetworkObject>(out var targetNetworkObj))
        {
            DealDamageServerRpc(targetNetworkObj, damageAmount);
        }

    }

    [ServerRpc]
    void DealDamageServerRpc(NetworkObjectReference targetRef, float amount)
    {
        if (targetRef.TryGet(out NetworkObject targetObj))
        {
            if (targetObj.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(amount);
            }
        }
    }


    void OnParticleCollision(GameObject other)
    {
        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(damageAmount);
        }
    }
}
