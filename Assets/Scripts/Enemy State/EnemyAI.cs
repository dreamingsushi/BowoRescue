using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public float detectionRadius = 10f;
    public float turnSpeed = 5f;
    public float attackRadius = 1.5f;
    public NavMeshAgent agent;
    private Rigidbody rb;
    private EnemyHealth enemyHealth;
    public bool inattackrange;
    public bool isStunned;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        enemyHealth = GetComponent<EnemyHealth>();
    }

    void Update()
    {
        if (enemyHealth.isDead) return;
        FindClosestPlayer();
        
        inattackrange = IsPlayerInAttackRange();
    }

    public void FindClosestPlayer()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        float closestDistance = Mathf.Infinity; // Start with an infinite distance
        Transform nearestPlayer = null;

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    nearestPlayer = collider.transform;
                }
            }
        }

        // If a nearest player is found, set it
        if (nearestPlayer != null)
        {
            player = nearestPlayer;
        }
    }


    public bool IsPlayerInRange()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsPlayerInAttackRange()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, attackRadius);
        
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }

    public void MoveTowardsPlayer()
    {
        if (!isStunned)
        {
            agent.SetDestination(player.position);
        }
    }

    public void RotateTowardsPlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
    }

    public IEnumerator ApplyKnockback(Vector3 force)
    {
        agent.enabled = false;
        rb.isKinematic = false;
        rb.useGravity = true;
        isStunned = true;

        Debug.Log($"[Knockback] Applying force: {force}");

        rb.AddForce(force, ForceMode.Impulse);

        yield return new WaitForFixedUpdate();

        float timeout = 2f;
        float startTime = Time.time;

        yield return new WaitUntil(() => 
            rb.linearVelocity.magnitude < 0.05f || Time.time - startTime > timeout
        );

        yield return new WaitForSeconds(0.25f);

        isStunned = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;
        agent.Warp(transform.position);
        agent.enabled = true;

        Debug.Log("[Knockback] Finished");
    }

}
