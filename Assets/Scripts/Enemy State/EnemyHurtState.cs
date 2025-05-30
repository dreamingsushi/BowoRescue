using System.Collections;
using UnityEngine;

public class EnemyHurtState : EnemyBaseState
{
    public EnemyHurtState(EnemyStateManager enemy) : base(enemy) { }

    public override void EnterState()
    {
        enemy.animator.SetTrigger("Hurt");
    }

    public override void UpdateState()
    {
        if (!enemy.enemyAI.isStunned)
        {
            enemy.TransitionToState(new EnemyChaseState(enemy));
        }
        else if (enemy.health.isDead)
        {
            enemy.TransitionToState(new EnemyDeadState(enemy));
        }
    }


    public override void ExitState()
    {
        
    }
}
