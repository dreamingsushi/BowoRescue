using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class PlayerHealth : NetworkBehaviour, IDamageable
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    
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
    public NetworkVariable<bool> isDead = new NetworkVariable<bool>(false);
    

    private Coroutine regenCoroutine;
    private PlayerTeleporter playerTeleporter;
    private PlayerController playerController;
    [SerializeField] private HealthBarUI healthBarUI;

    private void Start()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }

        if (canRegenerate && IsOwner)
        {
            regenCoroutine = StartCoroutine(RegenerateHealth());
        }

        playerTeleporter = GetComponent<PlayerTeleporter>();
        playerController = GetComponent<PlayerController>();
    }

    public void TakeDamage(float damageAmount)
    {
        if (!IsOwner) return;
        if (isInvincible || currentHealth.Value <= 0) return;

        RequestDamageServerRpc(damageAmount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDamageServerRpc(float damageAmount)
    {
        if (currentHealth.Value <= 0 || isDead.Value) return;

        PlayHurtAnimationClientRpc();

        float reducedDamage = damageAmount - armor;
        reducedDamage *= (1 - damageReductionPercent);
        reducedDamage = Mathf.Max(0, reducedDamage);

        currentHealth.Value -= Mathf.RoundToInt(reducedDamage);
        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);

        Debug.Log("Took damage: " + reducedDamage + " | Current Health: " + currentHealth.Value);

        UpdateHealthBarClientRpc(currentHealth.Value);

        if (currentHealth.Value <= 0)
        {
            DieServerRpc(); // already server-side
        }
        else
        {
            StartCoroutine(TriggerInvincibility());
        }
    }
    [ClientRpc]
    private void UpdateHealthBarClientRpc(int health)
    {
        if (healthBarUI != null)
        {
            healthBarUI.SetHealth(health, maxHealth);
        }
    }

    [ClientRpc]
    private void PlayHurtAnimationClientRpc()
    {
        if (anim != null)
        {
            anim.SetTrigger("Hurt");
        }
    }

    public void Heal(int amount)
    {
        if (!IsOwner) return;
        RequestHealServerRpc(amount);
    }

    [ServerRpc]
    private void RequestHealServerRpc(int amount)
    {
        if (isDead.Value) return;

        currentHealth.Value += amount;
        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);
        UpdateHealthBarClientRpc(currentHealth.Value);

        Debug.Log("Healed: " + amount + " | Current Health: " + currentHealth.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DieServerRpc()
    {
        isDead.Value = true;
        Debug.Log("Player has died.");

        // Disable player controls here if necessary
        playerController.DisableInputs();
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(5f);
        playerController.EnableInputs();
        // Reset health
        currentHealth.Value = maxHealth;
        isDead.Value = false;

        playerTeleporter.Teleport(playerTeleporter.teleportDestination);

        Debug.Log("Player respawned.");
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

            if (currentHealth.Value < maxHealth)
            {
                Heal(regenAmount);
            }
        }
    }
}
