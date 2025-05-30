using UnityEngine;
using UnityEngine.AI;

public class EnemyStateManager : MonoBehaviour
{
    public EnemyAI enemyAI;
    public Animator animator;
    public EnemyHealth health;
    private EnemyBaseState currentState;
    private float stateTimer;
    public float TimeInState => stateTimer;

    void Start()
    {
        TransitionToState(new EnemyIdleState(this));

        animator = GetComponentInChildren<Animator>();

        health = GetComponent<EnemyHealth>();
    }

    void Update()
    {
        stateTimer += Time.deltaTime;
        currentState?.UpdateState();
        
    }

    public void TransitionToState(EnemyBaseState newState)
    {
        currentState?.ExitState(); 
        currentState = newState;  
        stateTimer = 0f;       
        currentState?.EnterState();
    }

}
