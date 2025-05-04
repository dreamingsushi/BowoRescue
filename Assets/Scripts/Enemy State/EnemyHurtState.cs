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
    }


    public override void ExitState()
    {
        
    }
}
