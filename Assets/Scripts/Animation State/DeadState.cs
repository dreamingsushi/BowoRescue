using UnityEngine;

public class DeadState : BaseState
{
    public DeadState(PlayerStateMachine player) : base(player) { }

    public override void EnterState()
    {
        player.animator.SetTrigger("isDead");
    }

    public override void ExitState()
    {
        
    }

    public override void UpdateState()
    {
        if (player.health.isDead.Value) return;

        if (!player.controller.isWalking)
        {
            player.TransitionToState(new IdleState(player));
        }
        else if (player.controller.isWalking)
        {
            player.TransitionToState(new WalkState(player));
        }
        else if (player.playerAttack.isAttacking)
        {
            player.TransitionToState(new AttackState(player));
        }
        else if (player.controller.isJumping)
        {
            player.TransitionToState(new JumpState(player));
        }
    }

    public override BaseState GetNextState()
    {
        return null;
    }
}
