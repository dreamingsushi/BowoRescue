using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{
    public EnemyIdleState(EnemyStateManager enemy) : base(enemy) { }

    public override void EnterState()
    {
        enemy.animator.SetBool("IsIdle", true);
    }

    public override void UpdateState()
    {
        if (enemy.enemyAI.IsPlayerInRange())
        {
            enemy.TransitionToState(new EnemyChaseState(enemy));
        }
    }

    public override void ExitState() { 
        enemy.animator.SetBool("IsIdle", false);
    }
}

