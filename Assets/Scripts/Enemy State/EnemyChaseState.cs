using UnityEngine;

public class EnemyChaseState : EnemyBaseState
{
    public EnemyChaseState(Enemy enemy, Animator animator) : base(enemy, animator) { }

    public override void Enter()
    {
        animator.Play("Run");
    }

    public override void Update()
    {
        enemy.MoveToPlayer();

        if (enemy.IsInAttackRange())
        {
            enemy.TransitionToState(new EnemyAttackState(enemy, animator));
        }
    }

    public override void Exit() { }
}
