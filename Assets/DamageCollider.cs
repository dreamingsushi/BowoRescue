using UnityEngine;

public class DamageCollider : MonoBehaviour
{
    public float damageAmount;
    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IDamageable target))
        {
            target.TakeDamage(damageAmount, transform.position);
            Debug.Log("Attacked" + target);
        }
    }
}
