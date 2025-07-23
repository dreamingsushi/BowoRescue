using UnityEngine;

public class WalkState : BaseState
{
    public WalkState(PlayerStateMachine player) : base(player) { }
    public override void EnterState()
    {
        player.animator.SetBool("isWalking",true);
    }
    public override void ExitState()
    {
        player.animator.SetBool("isWalking",false);
    }
    public override void UpdateState()
    {
        if (!player.controller.isWalking)
        {
            player.TransitionToState(new IdleState(player));
        }
        else if (player.playerAttack.isAttacking)
        {
            player.TransitionToState(new AttackState(player));
        }
        else if (player.controller.isJumping)
        {
            player.TransitionToState(new JumpState(player));
        }
        else if (player.health.isInvincible.Value)
        {
            //player.TransitionToState(new HurtState(player));
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
