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
        inattackrange = IsPlayerInAttackRange();
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
        yield return null;
        agent.enabled = false;
        rb.useGravity = true;
        rb.isKinematic = false;
        isStunned = true;
        rb.AddForce(force);
        enemyHealth.TriggerHurtMaterial();

        yield return new WaitForFixedUpdate();
        float timeout = 1.5f;
        float knockbackTime= Time.time;
        yield return new WaitUntil(() => rb.linearVelocity.magnitude < 0.05f || Time.time - knockbackTime > timeout);
        yield return new WaitForSeconds(0.25f);

        isStunned = false;
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        agent.Warp(transform.position);
        agent.enabled = true;
        enemyHealth.BackToOriginalMaterial();

        yield return null;

        if (player != null)
        {

        }
    }
}
