using UnityEngine;

public class Chest : MonoBehaviour, IDamageable
{
    public float health = 40f;
    public float deathDelay = 2f; // Delay before destroying the object

    private ParticleSystem hitParticles;

    private void Awake()
    {
        // Get ParticleSystem from child
        hitParticles = GetComponentInChildren<ParticleSystem>();
    }

    public void TakeDamage(float damage)
    {
        // Play hit particles
        if (hitParticles != null)
        {
            hitParticles.Play();
        }

        // Apply damage
        health -= damage;

        // Check for death
        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        StartCoroutine(DelayedDestroy());
    }

    private System.Collections.IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(deathDelay);
        Destroy(gameObject);
    }
}
