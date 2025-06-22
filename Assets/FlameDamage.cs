using UnityEngine;

public class FlameDamage : MonoBehaviour
{
    public float damagePerSecond = 10f;

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(damagePerSecond * Time.deltaTime);
        }
    }
}
