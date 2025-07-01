using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

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
    [SerializeField] public HealthBarUI healthBarUI;

    public NetworkVariable<int> playerIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private Transform respawnArea;
    
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {

            // Set playerIndex using the server-side manager
            int index = PlayerIndexManager.Instance.GetPlayerIndex(OwnerClientId);
            playerIndex.Value = (int)OwnerClientId;

            currentHealth.Value = maxHealth;
        }

        if (canRegenerate && IsOwner)
        {
            regenCoroutine = StartCoroutine(RegenerateHealth());
        }

        playerTeleporter = GetComponent<PlayerTeleporter>();
        playerController = GetComponent<PlayerController>();

        StartCoroutine(DelayedRegisterToUI());
    }

    private IEnumerator DelayedRegisterToUI()
    {
        yield return new WaitUntil(() => IsSpawned && SceneManager.GetActiveScene().name == "Level 1");
        HealthBarManager manager = FindObjectOfType<HealthBarManager>();
        if (manager != null)
        {
            manager.RegisterPlayer(playerIndex.Value, this);
            Debug.Log("Index player: "+playerIndex.Value);
        }
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
        DisableInputsClientRpc(OwnerClientId);

        StartCoroutine(RespawnCoroutine());
    }

    [ClientRpc]
    private void DisableInputsClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            playerController.DisableInputs();
        }
    }

    [ClientRpc]
    private void EnableInputsClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            playerController.EnableInputs();
        }
    }


    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(5f);
        EnableInputsClientRpc(OwnerClientId);
        // Reset health
        currentHealth.Value = maxHealth;
        isDead.Value = false;

        Vector3 spawnPos = RespawnManager.Instance.GetSafeRespawnPosition();
        playerTeleporter.Teleport(spawnPos);
        UpdateHealthBarClientRpc(currentHealth.Value);
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
