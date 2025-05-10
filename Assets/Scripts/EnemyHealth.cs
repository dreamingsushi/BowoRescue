using UnityEngine;

[RequireComponent(typeof(EnemyAI))]
public class EnemyHealth : MonoBehaviour, IDamageable, IKnockbackable
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
        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }
    public void GetKnockedBack(Vector3 force)
    {
        StartCoroutine(enemyAI.ApplyKnockback(force));
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died.");
        // TODO: play death animation, disable AI, etc.
        Destroy(gameObject);
    }

    public void TriggerHurtMaterial()
    {
        mesh.material.color = Color.red;
    }

    public void BackToOriginalMaterial()
    {
        mesh.material.color = Color.white;
    }
}
