using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class Boss : NetworkBehaviour, IDamageable
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;
    public bool isDead = false;
    public SkinnedMeshRenderer mesh;

    [Header("Phase Thresholds")]
    public float phase2Threshold = 70f;
    public float phase3Threshold = 30f;

    public enum BossPhase {Idle, Phase1, Phase2, Phase3 }
    public BossPhase currentPhase = BossPhase.Phase1;
    private bool isInvulnerable = false;


    [Header("Movement Settings")]
    public float patrolSpeed = 5f;
    public float chaseSpeed = 3.5f;

    public float patrolRadius = 5f;
    public float patrolWaitTime = 2f;

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
    [Header("Spell Settings")]
    public GameObject[] spellPrefabs;

    [Header("Shield Settings")]
    public GameObject shieldEffect;

    [Header("Summon Settings")]
    public GameObject summonPrefab;
    public GameObject summonVFX;

    [Header("Camera")]
    public GameObject bossCamera;

    private Animator anim;

    void Start()
    {
        currentHealth = maxHealth;
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

        if (shieldEffect != null)
            shieldEffect.SetActive(false);        
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
                Patrol();
                break;
            case BossPhase.Phase2:
                Idle();
                break;
            case BossPhase.Phase3:
                ChaseAndAttack();
                break;
        }
    }

    public void TakeDamage(float damage)
    {
        if (!IsServer || isDead || isInvulnerable) return;

        currentHealth -= damage;

        TriggerHurtMaterialClientRpc();

        if (currentPhase == BossPhase.Phase1 && currentHealth <= phase2Threshold)
        {
            EnterPhase2();
        }
        else if (currentPhase == BossPhase.Phase2 && currentHealth <= phase3Threshold)
        {
            EnterPhase3();
        }

        if (currentHealth <= 0f)
        {
            StartCoroutine(Die());
        }
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
    }

    public void SummonDragon()
    {
        if (summonVFX != null)
        {
            summonVFX.SetActive(true);
        }

        if (summonPrefab != null)
        {
            summonPrefab.SetActive(true);
        }
    }

    private void EnterPhase2()
    {
        currentPhase = BossPhase.Phase2;
        isInvulnerable = true;

        if (shieldEffect != null)
            shieldEffect.SetActive(true);

        if (anim != null)
            anim.SetBool("IsShielded", true);

        SummonDragon();

        Debug.Log("Boss entered Phase 2 (Shielded)");
    }

    private void EnterPhase3()
    {
        currentPhase = BossPhase.Phase3;
        isInvulnerable = false;

        if (shieldEffect != null)
            shieldEffect.SetActive(false);
        
        agent.speed = chaseSpeed;

        if (anim != null)
            anim.SetBool("IsShielded", false);

        currentTargetIndex = -1; // reset so SelectNextTarget starts from 0
        SelectNextTarget();

        SummonDragon();

        Debug.Log("Boss entered Phase 3 (Chasing)");
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

                if (anim != null)
                    anim.SetBool("IsWalking", true);
            }
            else
            {
                if (anim != null)
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
        // do nothing, or play idle/shield animation
        if (anim != null)
        {
            anim.SetBool("IsWalking", false);
        }
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

        if (anim != null)
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
    }

    // --- ATTACK BEHAVIOUR ---

    void CastSpell()
    {
        Debug.Log("Boss is casting a random spell...");

        // Play casting animation
        if (anim != null)
            anim.SetTrigger("CastSpell");

        // Check if there are any spells
        if (spellPrefabs.Length == 0) return;

        // Choose one at random
        int index = Random.Range(0, spellPrefabs.Length);
        GameObject spellToCast = spellPrefabs[index];

        // Spawn the spell
        if (spellToCast != null && currentTarget != null)
        {
            Vector3 spawnPos = currentTarget.position; // 👈 Spawn at player's location
            GameObject spellInstance = Instantiate(spellToCast, spawnPos, Quaternion.identity);

            // Optional: Network spawn (if spell uses Netcode)
            if (spellInstance.TryGetComponent(out NetworkObject netObj))
            {
                netObj.Spawn();
            }
        }
    }

    void Spell_Snow()
    {

    }

    void Spell_Meteor()
    {

    }

    void Spell_RedEnergy()
    {

    }

    void AttackTarget()
    {
        Debug.Log($"Boss attacks {currentTarget.name}");

        // Optional: deal damage to player via interface
        var damageable = currentTarget.GetComponent<IDamageable>();
        if (damageable != null)
            damageable.TakeDamage(10f);

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

        if (agent != null)
            agent.enabled = false;

        if (anim != null)
            anim.SetBool("IsWalking", false);

        Debug.Log($"{gameObject.name} died... will be destroyed in 2 seconds.");
        DestroyBossClientRpc(); // sync death visuals to clients

        yield return new WaitForSeconds(2f); // delay

        Destroy(gameObject); // actual destroy after delay

        if (anim != null)
            anim.SetTrigger("DieTrigger");
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
