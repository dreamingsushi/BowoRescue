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
        StartCoroutine(DelayedDestroy());
    }

    private IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(deathDelay);

        DestroyLogClientRpc(); // Tell clients to destroy the object

        if (IsServer)
            NetworkObject.Despawn(); // Server removes the object from network
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
