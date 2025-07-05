using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class Log : NetworkBehaviour, IDamageable
{
    public float health = 40f;
    public float deathDelay = 2f;

    private ParticleSystem hitParticles;

    private void Awake()
    {
        hitParticles = GetComponentInChildren<ParticleSystem>();
    }

    public void TakeDamage(float damage)
    {
        RequestDamageServerRpc(damage);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestDamageServerRpc(float damage)
    {
        if (!IsServer) return; // Only the server processes damage

        health -= damage;

        PlayHitParticlesClientRpc(); // Tell all clients to play hit VFX

        if (health <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        StartCoroutine(SinkAndDestroy(4, 1.5f));
    }

    private IEnumerator SinkAndDestroy(float duration, float sinkDistance)
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.down * sinkDistance;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;

        if (IsServer && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(); // Proper networked despawn
        }
    }

    [ClientRpc]
    private void PlayHitParticlesClientRpc()
    {
        if (hitParticles != null)
            hitParticles.Play();
    }

    [ClientRpc]
    private void DestroyLogClientRpc()
    {
        // Destroy local GameObject on clients (in case it wasn't despawned properly)
        if (!IsServer) Destroy(gameObject);
    }


}
