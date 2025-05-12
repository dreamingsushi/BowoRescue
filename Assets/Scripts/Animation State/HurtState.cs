using UnityEngine;

public class HurtState : BaseState
{
    public HurtState(PlayerStateMachine player) : base(player) { }

    public override void EnterState()
    {
        // player.animator.SetTrigger("Hurt");
    }

    public override void ExitState()
    {
        
    }

    public override void UpdateState()
    {
        // if (!player.health.isInvincible) return;
        
        // if (player.controller.isWalking)
        // {
        //     player.TransitionToState(new WalkState(player));
        // }
        // else if (!player.controller.isWalking)
        // {
        //     player.TransitionToState(new IdleState(player));
        // }
        // else if (player.playerAttack.isAttacking)
        // {
        //     player.TransitionToState(new AttackState(player));
        // }
        // else if (player.controller.isJumping)
        // {
        //     player.TransitionToState(new JumpState(player));
        // }
    }

    public override BaseState GetNextState()
    {
        return null;
    }
}
