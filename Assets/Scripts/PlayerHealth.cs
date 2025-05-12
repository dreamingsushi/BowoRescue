using System.Collections;
using UnityEngine;
using Unity.Netcode;


public class PlayerHealth : NetworkBehaviour, IDamageable
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Armor Settings")]
    public int armor = 0;
    public float damageReductionPercent = 0.1f;

    [Header("Invincibility Settings")]
    public bool isInvincible = false;
    public float invincibilityDuration = 1f;

    [Header("Health Regeneration")]
    public bool canRegenerate = true;
    public int regenAmount = 1;
    public float regenInterval = 2f;
    [SerializeField] private Animator anim;
    private Coroutine regenCoroutine;

    void Start()
    {
        currentHealth = maxHealth;

        if (canRegenerate)
        {
            regenCoroutine = StartCoroutine(RegenerateHealth());
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (!IsOwner) return;
        if (isInvincible || currentHealth <= 0) return;

        anim.SetTrigger("Hurt");

        // Apply armor and damage reduction
        float reducedDamage = damageAmount - armor;
        reducedDamage *= (1 - damageReductionPercent);
        reducedDamage = Mathf.Max(0, reducedDamage);

        currentHealth -= Mathf.RoundToInt(reducedDamage);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Took damage: " + reducedDamage + " | Current Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(TriggerInvincibility());
        }
    }

    public void Heal(int amount)
    {
        if (!IsOwner) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log("Healed: " + amount + " | Current Health: " + currentHealth);
    }

    private void Die()
    {
        Debug.Log("Player has died.");
        // Add death handling here (e.g. respawn, disable controls, play animation)
    }

    private IEnumerator TriggerInvincibility()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    private IEnumerator RegenerateHealth()
    {
        while (true)
        {
            yield return new WaitForSeconds(regenInterval);

            if (currentHealth < maxHealth)
            {
                Heal(regenAmount);
            }
        }
    }

}