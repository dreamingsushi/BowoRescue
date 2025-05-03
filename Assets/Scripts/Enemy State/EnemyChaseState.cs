using UnityEngine;

public class EnemyChaseState : EnemyBaseState
{
    public EnemyChaseState(EnemyStateManager enemy) : base(enemy) { }

    public override void EnterState()
    {
        enemy.animator.SetBool("IsRunning", true);

        enemy.animator.SetBool("IsAttacking", false);
    }

    public override void UpdateState()
    {
        enemy.enemyAI.MoveTowardsPlayer();
        enemy.enemyAI.RotateTowardsPlayer();

        if (enemy.enemyAI.IsPlayerInAttackRange())
        {
            enemy.TransitionToState(new EnemyAttackState(enemy));
        }
    }

    public override void ExitState() 
    { 
        enemy.animator.SetBool("IsRunning", false);
    }
}
