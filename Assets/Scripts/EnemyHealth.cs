using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(EnemyAI))]
public class EnemyHealth : NetworkBehaviour, IDamageable, IKnockbackable
{
    public float maxHealth = 100f;
    private float currentHealth;
    public SkinnedMeshRenderer mesh;
    private EnemyAI enemyAI;

    void Start()
    {
        currentHealth = maxHealth;
        enemyAI = GetComponent<EnemyAI>();
    }

    public void TakeDamage(float damage)
    {
        if (!IsServer) return;

        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            Die();
        }

        TriggerHurtMaterialClientRpc();
    }

    public void GetKnockedBack(Vector3 force)
    {
        if (!IsServer) return;

        StartCoroutine(enemyAI.ApplyKnockback(force));
        TriggerHurtMaterialClientRpc();
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died.");
        DestroyEnemyClientRpc(); // sync death visuals to clients
        Destroy(gameObject); // server destroys object
    }

    [ClientRpc]
    private void DestroyEnemyClientRpc()
    {
        if (!IsServer) Destroy(gameObject); // client destroys local copy
    }

    [ClientRpc]
    public void TriggerHurtMaterialClientRpc()
    {
        mesh.material.color = Color.red;

        CancelInvoke(nameof(BackToOriginalMaterial));
        Invoke(nameof(BackToOriginalMaterial), 0.25f);
    }

    public void BackToOriginalMaterial()
    {
        mesh.material.color = Color.white;
    }
}
