using UnityEngine;

public class DamageCollider : MonoBehaviour
{
    public float damageAmount;
    public float knockbackStrength = 20f;
    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(damageAmount);
            Debug.Log("Attacked" + damageable);
        }

        if (other.TryGetComponent(out IKnockbackable knockbackable))
        {
            Vector3 direction = (other.transform.position - transform.position).normalized;
            Vector3 force = direction * knockbackStrength;
            knockbackable.GetKnockedBack(force);
        }
    }
}
