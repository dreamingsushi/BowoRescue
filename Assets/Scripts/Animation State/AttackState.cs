using UnityEngine;

public class AttackState : BaseState
{
    public AttackState(PlayerStateMachine player) : base(player) { }

    public override void EnterState()
    {
        player.animator.SetTrigger("Attack");
    }

    public override void ExitState()
    {
        
    }

    public override void UpdateState()
    {
        if (!player.playerAttack.isAttacking)
        {
            if (player.controller.isWalking)
            {
                player.TransitionToState(new WalkState(player));
            }
            else if (!player.controller.isWalking)
            {
                player.TransitionToState(new IdleState(player));
            }
        }
    }

    public override BaseState GetNextState()
    {
        return null;
    }
}