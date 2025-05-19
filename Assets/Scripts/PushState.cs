using UnityEngine;

public class PushState : BaseState
{
    public PushState(PlayerStateMachine player) : base(player) { }

    public override void EnterState()
    {
        player.animator.SetBool("isPushing",true);
    }

    public override void ExitState()
    {
        player.animator.SetBool("isPushing",false);
    }

    public override void UpdateState()
    {
        if (player.controller.isPushing) return;

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
