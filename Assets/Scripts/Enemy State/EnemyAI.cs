using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public float detectionRadius = 10f;
    public float turnSpeed = 5f;
    public float attackRadius = 1.5f;
    public NavMeshAgent agent;
    public bool inattackrange;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
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
        agent.SetDestination(player.position);
    }

    public void RotateTowardsPlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
    }
}
