using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{
    public EnemyIdleState(Enemy enemy, Animator animator) : base(enemy, animator) { }

    public override void Enter()
    {
        animator.Play("Idle");
    }

    public override void Update()
    {
        if (enemy.IsPlayerInRange())
        {
            enemy.TransitionToState(new EnemyChaseState(enemy, animator));
        }
    }

    public override void Exit() { }
}

