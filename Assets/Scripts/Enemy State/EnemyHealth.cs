using UnityEngine;
using Unity.Netcode;
using System.Collections;

[RequireComponent(typeof(EnemyAI))]
public class EnemyHealth : NetworkBehaviour, IDamageable, IKnockbackable
{
    public float maxHealth = 100f;
    private float currentHealth;
    public SkinnedMeshRenderer mesh;
    private EnemyAI enemyAI;
    public bool isDead = false;
    [SerializeField] private GameObject hitVFXPrefab;

    void Start()
    {
        currentHealth = maxHealth;
        enemyAI = GetComponent<EnemyAI>();
        isDead = false;
    }

    public void TakeDamage(float damage)
    {
        if (!IsServer)
        {
            TakeDamageServerRpc(damage);
        }
        else
        {
            ApplyDamage(damage);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage)
    {
        ApplyDamage(damage);
    }

    // Only runs on server
    private void ApplyDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        SpawnHitVFXClientRpc();

        TriggerHurtMaterialClientRpc();

        if (currentHealth <= 0f)
        {
            StartCoroutine(Die());
        }
    }

    [ClientRpc]
    private void SpawnHitVFXClientRpc()
    {
        if (hitVFXPrefab == null) return;

        GameObject vfx = Instantiate(hitVFXPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
        Destroy(vfx, 2f); // Clean up after 2 seconds
    }



    public void GetKnockedBack(Vector3 force)
    {
        if (!IsServer)
        {
            GetKnockedBackServerRpc(force);
        }
        else
        {
            ApplyKnockback(force);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void GetKnockedBackServerRpc(Vector3 force)
    {
        ApplyKnockback(force);
    }

    private void ApplyKnockback(Vector3 force)
    {
        if (isDead) return;

        StartCoroutine(enemyAI.ApplyKnockback(force));
        TriggerHurtMaterialClientRpc(); // optional red flash / feedback
    }

    private IEnumerator Die()
    {
        isDead = true;

        var drop = GetComponent<EnemyDrop>();
        bool willDrop = drop != null;

        if (willDrop)
        {
            drop.TrySpawnDrop();

            Debug.Log($"{gameObject.name} dropped something — destroying immediately.");

            DestroyEnemyClientRpc();
            Destroy(gameObject); // no delay
            yield break; // end coroutine early
        }
        StartCoroutine(SinkAndDestroy(4f, 1.5f));
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
        else
        {
            DestroyEnemyClientRpc();
        }
    }


    [ClientRpc]
    private void DestroyEnemyClientRpc()
    {
        if (!IsServer) Destroy(gameObject); // client destroys local copy
    }

    [ClientRpc]
    public void TriggerHurtMaterialClientRpc()
    {
        if (mesh == null) return;

        mesh.material.color = Color.red;

        CancelInvoke(nameof(BackToOriginalMaterial));
        Invoke(nameof(BackToOriginalMaterial), 0.25f);
    }

    public void BackToOriginalMaterial()
    {
        if (mesh == null) return;

        mesh.material.color = Color.white;
    }
}
