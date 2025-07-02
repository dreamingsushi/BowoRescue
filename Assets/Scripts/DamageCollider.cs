using Unity.Netcode;
using UnityEngine;

public class DamageCollider : MonoBehaviour
{
    public GameObject hitVFXPrefab;
    public float damageAmount = 10f;
    public float knockbackStrength = 20f;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return; // Only server handles damage

        if (gameObject.CompareTag("Enemy") && other.CompareTag("Enemy"))
            return;

        if (other.TryGetComponent(out NetworkObject netObj))
        {
            DealDamageAndKnockback(netObj, other.ClosestPoint(transform.position));
        }
    }

    private void DealDamageAndKnockback(NetworkObject targetObj, Vector3 hitPoint)
    {
        if (targetObj.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(damageAmount);
            Debug.Log($"[Server] Damaged {targetObj.name} for {damageAmount}");

            // Optional: Camera shake (must call via ClientRpc)
            CameraShakeClientRpc();
            HitStopManager.Instance?.DoHitStop(0.1f);
        }

        if (targetObj.TryGetComponent(out IKnockbackable knockbackable))
        {
            Vector3 direction = (targetObj.transform.position - transform.position).normalized;
            Vector3 force = direction * knockbackStrength;
            knockbackable.GetKnockedBack(force);
        }

        if (hitVFXPrefab != null)
        {
            SpawnHitVFXClientRpc(hitPoint);
        }
    }

    [ClientRpc]
    private void SpawnHitVFXClientRpc(Vector3 hitPoint)
    {
        if (hitVFXPrefab != null)
        {
            Instantiate(hitVFXPrefab, hitPoint, Quaternion.identity);
        }
    }

    [ClientRpc]
    private void CameraShakeClientRpc()
    {
        CameraShakeManager.Instance?.Shake();
    }

    private bool IsServer => NetworkManager.Singleton.IsServer;
}
