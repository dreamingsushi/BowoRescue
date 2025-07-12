using UnityEngine;
using Unity.Netcode;

public class Log : NetworkBehaviour, IDamageable
{
    public NetworkVariable<float> Health = new NetworkVariable<float>(
        40f, // default value
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

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
        Health.Value -= damage;

        PlayHitParticlesClientRpc(); // Visual feedback

        if (Health.Value <= 0f)
        {
            DestroyLogClientRpc(); // Tell clients to destroy
            Destroy(gameObject);   // Destroy on server
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
        if (!IsServer)
            Destroy(gameObject);
    }
}
