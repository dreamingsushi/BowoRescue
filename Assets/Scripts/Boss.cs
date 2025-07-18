using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class Boss : NetworkBehaviour, IDamageable
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public NetworkVariable<float> currentHealth = new NetworkVariable<float>(
    100f,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
    );
    public bool isDead = false;
    public SkinnedMeshRenderer mesh;
    [SerializeField] private GameObject hitVFXPrefab;

    [Header("Phase Thresholds")]
    public float phase2Threshold = 70f;
    public float phase3Threshold = 30f;

    public enum BossPhase { Idle, Phase1, Phase2, Phase3 }
    public BossPhase currentPhase = BossPhase.Phase1;
    private bool isInvulnerable = false;


    [Header("Movement Settings")]
    public float patrolSpeed = 5f;
    public float chaseSpeed = 3.5f;

    public float patrolRadius = 5f;
    public float patrolWaitTime = 2f;
    [Header ("Teleport Settings")]
    public Transform bossTP;
    public GameObject teleportVFX;
    private Vector3 patrolStartPos;
    private Vector3 patrolTarget;
    private float patrolTimer = 0f;
    private bool hasPatrolTarget = false;

    private NavMeshAgent agent;
    private Transform currentTarget;
    private List<Transform> allPlayers = new List<Transform>();
    private int currentTargetIndex = 0;

    [Header("Patrol Area Settings")]
    public float mapSize = 50f;
    public float minPatrolDistance = 3f;
    public Transform patrolCenter;

    [Header("Attack Settings")]
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    private float attackTimer = 0f;
    [Header("Phase 3 Spell")]
    public float spellCastInterval = 10f;
    private float spellCastTimer = 0f;

    [Header("Spell Settings")]
    public GameObject[] spellPrefabs;
    public GameObject spellIndicatorPrefab;
    [Header("Spell Sounds")]
    public string[] spellSFXNames; 

    [Header("Shield Settings")]
    public GameObject shieldEffect;

    [Header("Summon Settings")]
    public GameObject dragonSummonPrefab;
    public GameObject dragonSummonVFX;
    public Transform dragonSummonVFXSpawnpoints;
    [SerializeField] private Transform dragonSummonPoint;
    public GameObject slimePrefab;

    [Header("Camera")]
    public GameObject bossCamera;
    private Animator anim;


    void Start()
    {
        currentHealth.Value = maxHealth;
        isDead = false;
        patrolStartPos = transform.position;

        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = true;
            agent.speed = patrolSpeed;
        }

        anim = GetComponent<Animator>();
        if (anim != null)
            anim.SetBool("IsWalking", false);
    }

    private void GatherPlayers()
    {
        foreach (var netObj in NetworkManager.Singleton.ConnectedClientsList)
        {
            GameObject playerObj = netObj.PlayerObject?.gameObject;
            if (playerObj != null)
                allPlayers.Add(playerObj.transform);
        }

        if (allPlayers.Count == 0)
            Debug.LogWarning("Boss could not find any players!");
        else
            Debug.Log($"Boss found {allPlayers.Count} players.");
    }


    void Update()
    {
        if (!IsServer || isDead) return;

        switch (currentPhase)
        {
            case BossPhase.Idle:
                break;
            case BossPhase.Phase1:
                ChaseAndAttack();
                break;
            case BossPhase.Phase2:
                Idle();
                HandleSpellCasting();
                break;
            case BossPhase.Phase3:
                Patrol();
                break;
        }
    }

    [ServerRpc (RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage)
    {
        if (!IsServer || isDead || isInvulnerable) return;

        currentHealth.Value -= damage;

        TriggerHurtMaterialClientRpc();
        SpawnHitVFXClientRpc();

        if (currentPhase == BossPhase.Phase1 && currentHealth.Value <= phase2Threshold)
        {
            EnterPhase2();
        }
        else if (currentPhase == BossPhase.Phase2 && currentHealth.Value <= phase3Threshold)
        {
            EnterPhase3();
        }

        if (currentHealth.Value <= 0f)
        {
            StartCoroutine(Die());
        }
    }

    public void TakeDamage(float damage)
    {
        TakeDamageServerRpc(damage);
    }

    [ClientRpc]
    private void SpawnHitVFXClientRpc()
    {
        if (hitVFXPrefab == null) return;

        GameObject vfx = Instantiate(hitVFXPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
        Destroy(vfx, 2f); // Clean up after 2 seconds
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

    // --- PHASE BEHAVIORS ---

    public void StartPhase1()
    {
        bossCamera.SetActive(true);
        currentPhase = BossPhase.Phase1;
        GatherPlayers();
        agent.speed = chaseSpeed; // chasing
        SelectNextTarget();       // immediately pick someone
    }

    public void SummonDragon()
    {
        if (!IsServer) return; // Only server should spawn

        AudioManager.Instance.PlaySFX("Dragon");
        if (dragonSummonVFX != null)
        {
            GameObject vfx = Instantiate(dragonSummonVFX, dragonSummonVFXSpawnpoints.position, Quaternion.identity);
            if (vfx.TryGetComponent(out NetworkObject vfxNet))
                vfxNet.Spawn();

            StartCoroutine(DespawnVFXAfterDelay(vfxNet, 3f));
            Destroy(vfx, 3f);
        }

        if (dragonSummonPrefab != null)
        {
            GameObject dragon = Instantiate(dragonSummonPrefab, dragonSummonPoint.position, Quaternion.identity);

            if (dragon.TryGetComponent(out NetworkObject netObj))
                netObj.Spawn();
            else
                Debug.LogError("Summon prefab is missing a NetworkObject!");
        }
    }

    private IEnumerator DespawnVFXAfterDelay(NetworkObject netObj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (netObj != null && netObj.IsSpawned)
        {
            netObj.Despawn();
        }
    }

    private void EnterPhase2()
    {
        currentPhase = BossPhase.Phase2;
        isInvulnerable = true;

        ToggleShieldEffectClientRpc(true);

        anim.SetBool("IsShielded", true);

        StartCoroutine(SpawnMonstersLoop());
        TeleportBoss(bossTP);
        spellCastTimer = 0f;
        Debug.Log("Boss entered Phase 2 (Shielded)");
    }

    public void BreakShieldFromBomb()
    {
        if (!isInvulnerable) return;
        isInvulnerable = false;
        anim.SetBool("IsShielded", false);
        anim.SetBool("IsDizzy", true);
        ToggleShieldEffectClientRpc(false);
        Debug.Log("Boss shield broken by bomb!");
    }


    private void EnterPhase3()
    {
        anim.SetBool("IsDizzy", false);
        currentPhase = BossPhase.Phase3;
        isInvulnerable = false;

        ToggleShieldEffectClientRpc(false);

        agent.speed = patrolSpeed;

        currentTargetIndex = -1; // reset so SelectNextTarget starts from 0
        SelectNextTarget();

        SummonDragon();

        Debug.Log("Boss entered Phase 3 (Chasing)");
    }

    [ClientRpc]
    private void ToggleShieldEffectClientRpc(bool isActive)
    {
        if (shieldEffect != null)
            shieldEffect.SetActive(isActive);
    }


    // --- MOVEMENT BEHAVIORS ---

    void Patrol()
    {
        if (agent == null || !agent.isOnNavMesh) return;

        agent.speed = patrolSpeed;

        if (!hasPatrolTarget || Vector3.Distance(transform.position, patrolTarget) < 0.5f)
        {
            patrolTimer += Time.deltaTime;

            if (patrolTimer >= patrolWaitTime)
            {
                patrolTarget = GetRandomPatrolPoint();
                agent.SetDestination(patrolTarget);

                SelectNextTarget();
                CastSpell();

                hasPatrolTarget = true;
                patrolTimer = 0f;

                anim.SetBool("IsWalking", true);
            }
            else
            {
                anim.SetBool("IsWalking", false);
            }
        }

        // Rotate toward movement direction
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    Vector3 GetRandomPatrolPoint()
    {
        Vector3 center = patrolCenter != null ? patrolCenter.position : transform.position;

        for (int i = 0; i < 20; i++)
        {
            Vector3 randomPoint = center + new Vector3(
                Random.Range(-mapSize, mapSize),
                0,
                Random.Range(-mapSize, mapSize)
            );

            if (Vector3.Distance(transform.position, randomPoint) < minPatrolDistance)
                continue; // too close, skip

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        Debug.LogWarning("Failed to find valid distant patrol point.");
        return transform.position;
    }


    void Idle()
    {
        anim.SetBool("IsWalking", false);
    }

    public void TeleportBoss(Transform destination)
    {
        if (!IsServer) return;

        // Spawn VFX at current position before teleport
        SpawnTeleportVFXClientRpc(transform.position);

        agent.Warp(destination.position);
        currentTarget = null;

        SpawnTeleportVFXClientRpc(destination.position);

        Debug.Log($"Boss teleported to {destination}");
    }

    [ClientRpc]
    private void SpawnTeleportVFXClientRpc(Vector3 position)
    {
        if (teleportVFX != null)
        {
            GameObject vfx = Instantiate(teleportVFX, position, Quaternion.identity);
            Destroy(vfx, 2f); // Auto-destroy after 2 seconds
        }
    }

    // --- ATTACK BEHAVIOUR ---

    void CastSpell()
    {
        Debug.Log("Boss is casting a random spell...");

        // Check if there are any spells
        if (spellPrefabs.Length == 0) return;

        // Choose one at random
        int index = Random.Range(0, spellPrefabs.Length);
        GameObject spellToCast = spellPrefabs[index];

        // Play spell SFX
        if (spellSFXNames != null && index < spellSFXNames.Length)
        {
            AudioManager.Instance.PlaySFX(spellSFXNames[index]);
        }
        else
        {
            Debug.LogWarning("No spell SFX defined for this spell index.");
        }

        StartCoroutine(CastSpellWithDelay(spellToCast, currentTarget.position));
    }

    private IEnumerator CastSpellWithDelay(GameObject spellPrefab, Vector3 targetPosition)
    {
        if (spellIndicatorPrefab != null)
        {
            Quaternion rot = Quaternion.Euler(-90f, 0f, 0f);
            GameObject indicator = Instantiate(spellIndicatorPrefab, targetPosition, rot);
            Destroy(indicator, 1f);
        }

        anim.SetTrigger("CastSpell"); // play cast animation immediately

        yield return new WaitForSeconds(1f); // delay before spell spawns

        if (spellPrefab != null)
        {
            GameObject spellInstance = Instantiate(spellPrefab, targetPosition, Quaternion.identity);

            if (spellInstance.TryGetComponent(out NetworkObject netObj))
                netObj.Spawn();
        }
    }


    private IEnumerator SpawnMonstersLoop()
    {
        while (!isDead && currentPhase == BossPhase.Phase2)
        {
            for (int i = 0; i < 2; i++)
            {
                SpawnMonstersWithinMap();
            }
            if (currentPhase == BossPhase.Phase2)
            {
                yield return new WaitForSeconds(15f);
            }
            else
            {
                yield return new WaitForSeconds(30f);
            }

        }
    }

    public void SpawnMonstersWithinMap()
    {
        if (slimePrefab == null) return;

        Vector3 center = patrolCenter != null ? patrolCenter.position : transform.position;

        for (int i = 0; i < 20; i++) // max attempts
        {
            Vector3 randomPoint = center + new Vector3(
                Random.Range(-mapSize, mapSize),
                0,
                Random.Range(-mapSize, mapSize)
            );

            // Place on NavMesh or ground
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                GameObject slime = Instantiate(slimePrefab, hit.position, Quaternion.identity);

                if (slime.TryGetComponent(out NetworkObject netObj))
                    netObj.Spawn();

                Debug.Log($"Spawned slime at {hit.position}");
                return;
            }
        }

        Debug.LogWarning("Failed to spawn bomb: no valid point found");
    }

    void ChaseAndAttack()
    {
        if (agent == null || !agent.enabled) return;

        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
        {
            SelectNextTarget();
            return;
        }

        float distance = Vector3.Distance(transform.position, currentTarget.position);
        agent.SetDestination(currentTarget.position);

        anim.SetBool("IsWalking", true);


        // Smooth rotation
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        attackTimer += Time.deltaTime;

        if (distance <= attackRange && attackTimer >= attackCooldown)
        {
            AttackTarget();
            attackTimer = 0f;
        }

        // --- Spell Cast ---
        spellCastTimer += Time.deltaTime;
        if (spellCastTimer >= spellCastInterval)
        {
            CastSpell();
            spellCastTimer = 0f;
        }
    }

    private void HandleSpellCasting()
    {
        spellCastTimer += Time.deltaTime;

        if (spellCastTimer >= spellCastInterval)
        {
            if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
            {
                SelectNextTarget(); // ?? ensure you have a valid target
            }

            if (currentTarget != null)
            {
                CastSpell();
                spellCastTimer = 0f;
            }
        }
    }

    void AttackTarget()
    {
        Debug.Log($"Boss attacks {currentTarget.name}");

        anim.SetTrigger("MeleeAttack");

        AudioManager.Instance.PlaySFX("Spell");
        
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
        }

        StartCoroutine(DealMeleeDamageWithDelay(0.5f));

        //SelectNextTarget();
    }



    private IEnumerator DealMeleeDamageWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (currentTarget != null && currentTarget.gameObject.activeInHierarchy)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.position);
            if (distance <= attackRange)
            {
                if (currentTarget.TryGetComponent(out IDamageable target))
                {
                    target.TakeDamage(15f); // damage value
                    Debug.Log($"Boss hits {currentTarget.name} with melee.");
                }
            }
        }
        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
        }

        SelectNextTarget();
    }


    void SelectNextTarget()
    {
        if (allPlayers.Count == 0) return;

        for (int i = 0; i < allPlayers.Count; i++)
        {
            currentTargetIndex = (currentTargetIndex + 1) % allPlayers.Count;
            var candidate = allPlayers[currentTargetIndex];

            if (candidate != null && candidate.gameObject.activeInHierarchy)
            {
                currentTarget = candidate;
                Debug.Log($"Boss switches to {currentTarget.name}");
                return;
            }
        }

        currentTarget = null;
    }

    // --- DEATH ---

    private IEnumerator Die()
    {
        if (isDead) yield break;

        isDead = true;

        anim.SetTrigger("DieTrigger");
        AudioManager.Instance.PlaySFX("Cinematic");
        AudioManager.Instance.PlaySFX("BossDie");
        AudioManager.Instance.PlayMusic("EndTheme");
        agent.enabled = false;

        anim.SetBool("IsWalking", false);

        Debug.Log($"{gameObject.name} died... will be destroyed in 2 seconds.");
        DestroyBossClientRpc(); // sync death visuals to clients
    }

    [ClientRpc]
    private void DestroyBossClientRpc()
    {
        if (!IsServer)
            Destroy(gameObject); // client destroys local copy
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // semi-transparent red
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }


}
