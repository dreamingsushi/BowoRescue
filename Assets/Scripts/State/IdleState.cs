using Unity.VisualScripting;
using UnityEngine;

public class IdleState : BaseState
{
    public IdleState(PlayerStateMachine player) : base(player) { }

    public override void EnterState()
    {
        
    }

    public override void ExitState()
    {

    }

    public override void UpdateState()
    {
        if (player.controller.isWalking)
        {
            player.TransitionToState(new WalkState(player));
        }
        else if (player.playerAttack.isAttacking)
        {
            player.TransitionToState(new AttackState(player));
        }
    }

    public override BaseState GetNextState()
    {
        return null;
    }
}