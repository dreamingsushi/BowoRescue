using System.Collections;
using UnityEngine;

public class EnemyAttackState : EnemyBaseState
{
    private bool isAttacking = false;
    public EnemyAttackState(EnemyStateManager enemy) : base(enemy) { }

    public override void EnterState()
    {
        enemy.animator.SetBool("IsAttacking" , true);
        enemy.enemyAI.agent.isStopped = true;
        enemy.StartCoroutine(AttackLoop());
    }

    public override void UpdateState()
    {
        if (!enemy.enemyAI.IsPlayerInAttackRange() && !isAttacking)
        {
            enemy.animator.SetBool("IsAttacking" , false);
            enemy.TransitionToState(new EnemyChaseState(enemy));
        }
        else
        {
            enemy.enemyAI.RotateTowardsPlayer();
        }

        if (enemy.enemyAI.isStunned)
        {
            enemy.TransitionToState(new EnemyHurtState(enemy));
        }
    }


    public override void ExitState()
    {
        enemy.enemyAI.agent.isStopped = false;
    }

    private IEnumerator AttackLoop()
    {
        while(enemy.enemyAI.IsPlayerInAttackRange())
        {
            Attack();
            yield return new WaitForSeconds(1f);
            isAttacking = false;
        }
    }

    private void Attack()
    {
        isAttacking = true;
        enemy.animator.SetTrigger("Attack");
    }
}
