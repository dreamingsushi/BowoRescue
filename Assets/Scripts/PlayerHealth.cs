using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerHealth : NetworkBehaviour, IDamageable
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    [SerializeField] private TextMeshProUGUI respawnTimerText;
    
    [Header("Armor Settings")]
    public int armor = 0;
    public float damageReductionPercent = 0.1f;

    [Header("Invincibility Settings")]
    public NetworkVariable<bool> isInvincible = new NetworkVariable<bool>(
    false,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
    );

    public float invincibilityDuration = 0.2f;

    [Header("Health Regeneration")]
    public bool canRegenerate = true;
    public int regenAmount = 1;
    public float regenInterval = 1f;

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

    public IEnumerator DelayedRegisterToUI()
    {
        yield return new WaitForSeconds(5f);
        int index = PlayerIndexManager.Instance.GetPlayerIndex(OwnerClientId);
        Debug.Log("index is " + index);

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
        if (isInvincible.Value || currentHealth.Value <= 0) return;

        RequestDamageServerRpc(damageAmount);
        HitStopManager.Instance?.DoHitStop(0.1f);
        CameraShakeManager.Instance?.Shake();
        AudioManager.Instance.PlaySFX("Hurt");
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
            TriggerInvincibility();
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
        
        if (TeamLivesManager.Instance != null)
        {
            TeamLivesManager.Instance.ReduceLifeServerRpc();
        }
        else
        {
            Debug.LogWarning("TeamLivesManager.Instance is NULL. Skipping ReduceLifeServerRpc.");
        }

        // Disable player controls here if necessary
        DisableInputsClientRpc(OwnerClientId);
        DisableInput2ClientRpc();

        StartRespawnClientRpc();
    }

    [ClientRpc]
    private void DisableInputsClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            playerController.DisableInputsClientRpc();
        }
    }

    [ClientRpc]
    private void EnableInputsClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            playerController.EnableInputsClientRpc();
        }
    }

    [ClientRpc]
    private void EnableInput2ClientRpc()
    {
        playerController.EnableInputs();
    }

    [ClientRpc]
    private void DisableInput2ClientRpc()
    {
        playerController.DisableInputs();
    }

    [ClientRpc]
    private void StartRespawnClientRpc()
    {
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        float respawnTime = 5f;

        if (respawnTimerText != null)
        {
            respawnTimerText.gameObject.SetActive(true);
            ShowDeathUIClientRpc();
        }

        while (respawnTime > 0f)
        {
            string display = "Respawning in: " + Mathf.CeilToInt(respawnTime);
            if (respawnTimerText != null)
            {
                respawnTimerText.text = display;
                UpdateRespawnTextClientRpc(display);
            }

            yield return new WaitForSeconds(1f);
            respawnTime -= 1f;
        }

        if (respawnTimerText != null)
        {
            respawnTimerText.gameObject.SetActive(false);
            HideDeathUIClientRpc();
        }

        EnableInputsClientRpc(OwnerClientId);
        EnableInput2ClientRpc();

        currentHealth.Value = maxHealth;
        isDead.Value = false;

        Vector3 spawnPos = RespawnManager.Instance.GetSafeRespawnPosition();
        playerTeleporter.Teleport(spawnPos);
        UpdateHealthBarClientRpc(currentHealth.Value);

        Debug.Log("Player respawned.");
    }

    [ClientRpc]
    private void ShowDeathUIClientRpc()
    {
        respawnTimerText.gameObject.SetActive(true);
    }

    [ClientRpc]
    private void HideDeathUIClientRpc()
    {
        respawnTimerText.gameObject.SetActive(false);
    }

    [ClientRpc]
    private void UpdateRespawnTextClientRpc(string text)
    {
        if (respawnTimerText != null)
        {
            respawnTimerText.text = text;
        }
    }


    public void TriggerInvincibility()
    {
        if (IsServer)
            StartCoroutine(InvincibilityCoroutine());
        else
            TriggerInvincibilityServerRpc();
    }

    [ServerRpc (RequireOwnership = false)]
    private void TriggerInvincibilityServerRpc()
    {
        if (!isDead.Value)
        {
            StartCoroutine(InvincibilityCoroutine());
        }
    }


    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible.Value = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible.Value = false;
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
