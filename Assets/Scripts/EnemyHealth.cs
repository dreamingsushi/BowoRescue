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

    void Start()
    {
        currentHealth = maxHealth;
        enemyAI = GetComponent<EnemyAI>();
        isDead = false;
    }

    public void TakeDamage(float damage)
    {
        if (!IsServer || isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            StartCoroutine(Die());
        }

        TriggerHurtMaterialClientRpc();
    }

    public void GetKnockedBack(Vector3 force)
    {
        if (!IsServer || isDead) return;

        StartCoroutine(enemyAI.ApplyKnockback(force));
        TriggerHurtMaterialClientRpc();
    }

    private IEnumerator Die()
    {
        if (isDead) yield break;

        isDead = true;
        Debug.Log($"{gameObject.name} died... will be destroyed in 2 seconds.");
        DestroyEnemyClientRpc(); // sync death visuals to clients

        yield return new WaitForSeconds(2f); // delay

        Destroy(gameObject); // actual destroy after delay
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
