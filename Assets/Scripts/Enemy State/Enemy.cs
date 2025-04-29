using UnityEngine;

public class Enemy : MonoBehaviour
{
    private EnemyBaseState currentState;
    public Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        currentState = new EnemyIdleState(this, animator);
        currentState.Enter();
    }

    void Update()
    {
        currentState.Update();
    }

    public void TransitionToState(EnemyBaseState newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }

    // Your own methods:
    public bool IsPlayerInRange() { /* your logic */ return false; }
    public bool IsInAttackRange() { /* your logic */ return false; }
    public void MoveToPlayer() { /* move logic */ }
}
