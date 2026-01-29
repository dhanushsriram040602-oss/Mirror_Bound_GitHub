using UnityEngine;

public class MoveState : IState
{
    readonly PlayerCtrl player;
    readonly StateMachine fsm;

    public MoveState(PlayerCtrl player, StateMachine fsm)
    {
        this.player = player;
        this.fsm = fsm;
    }

    public void Enter() { }

    public void Update()
    {
        if (player.TryConsumeJump())
        {
            fsm.ChangeState(new JumpState(player, fsm));
            return;
        }

        if (!player.IsGrounded())
        {
            fsm.ChangeState(new AirState(player, fsm));
            return;
        }

        if (Mathf.Abs(PlayerInput.Instance.InputX) < 0.01f)
        {
            fsm.ChangeState(new IdleState(player, fsm));
        }
    }


    public void FixedUpdate()
    {
        player.TickMovement(Time.fixedDeltaTime);
        player.TickGravity(Time.fixedDeltaTime);
    }

    public void Exit() { }
}
