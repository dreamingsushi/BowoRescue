using UnityEngine;
using UnityEngine.AI;

public class EnemyStateManager : MonoBehaviour
{
    public EnemyAI enemyAI;
    public Animator animator;
    private EnemyBaseState currentState;
    private float stateTimer;
    public float TimeInState => stateTimer;

     void Start()
    {
        TransitionToState(new EnemyIdleState(this));
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
