using UnityEngine;

public class JumpState : BaseState
{
    public JumpState(PlayerStateMachine player) : base(player) { }

    public override void EnterState()
    {
        player.animator.SetTrigger("Jump");
    }

    public override void ExitState()
    {
        
    }

    public override void UpdateState()
    {
        if (!player.controller.controller.isGrounded) return;

        if (!player.controller.isWalking)
        {
            player.TransitionToState(new IdleState(player));
        }
        else if (player.controller.isWalking)
        {
            player.TransitionToState(new WalkState(player));
        }
        else if (player.controller.isPushing)
        {
            player.TransitionToState(new PushState(player));
        }
        else if (player.health.isDead.Value)
        {
            player.TransitionToState(new DeadState(player));
        }
    }

    public override BaseState GetNextState()
    {
        return null;
    }
}
