using UnityEngine;

public class EnemyAttackState : EnemyBaseState
{
    public EnemyAttackState(Enemy enemy, Animator animator) : base(enemy, animator) { }

    public override void Enter()
    {
        animator.Play("Attack");
    }

    public override void Update()
    {
        if (!enemy.IsInAttackRange())
        {
            enemy.TransitionToState(new EnemyChaseState(enemy, animator));
        }
    }

    public override void Exit() { }
}
