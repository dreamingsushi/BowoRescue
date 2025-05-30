using UnityEngine;

public class EnemyDeadState : EnemyBaseState
{
    public EnemyDeadState(EnemyStateManager enemy) : base(enemy) { }

    public override void EnterState()
    {
        enemy.animator.SetTrigger("Die");
    }

    public override void UpdateState()
    {
        
    }

    public override void ExitState() { 
        
    }
}

