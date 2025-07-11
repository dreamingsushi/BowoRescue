using UnityEngine;
using Unity.Netcode;

public class Spell : NetworkBehaviour
{
    [SerializeField] private ParticleSystem vfx;

    public override void OnNetworkSpawn()
    {
        if (vfx == null)
            vfx = GetComponent<ParticleSystem>();

        if (IsClient)
        {
            vfx?.Play();
            Debug.Log($"[Client] Spell spawned for client {OwnerClientId}");
        }

        if (IsServer && vfx != null)
        {
            // Auto-despawn after VFX finishes
            float duration = vfx.main.duration + vfx.main.startLifetime.constantMax;
            Invoke(nameof(DespawnSpell), duration);
        }
    }

    private void DespawnSpell()
    {
        if (IsServer && NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }
}
