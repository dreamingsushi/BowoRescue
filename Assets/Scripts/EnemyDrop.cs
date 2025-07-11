using UnityEngine;
using Unity.Netcode;

public class EnemyDrop : NetworkBehaviour
{
    [Header("Drop Settings")]
    public GameObject dropPrefab;
    public float dropChance = 1f;

    public void TrySpawnDrop()
    {
        if (!IsServer) return;
        if (dropPrefab == null || Random.value > dropChance) return;

        GameObject drop = Instantiate(dropPrefab, transform.position, Quaternion.identity);

        if (drop.TryGetComponent(out NetworkObject netObj))
            netObj.Spawn();

        Debug.Log($"[Drop] Spawned {drop.name} at {transform.position}");
    }
}
