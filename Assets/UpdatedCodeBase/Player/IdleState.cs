using UnityEngine;

public class IdleState : IState
{
    PlayerCtrl p;
    StateMachine fsm;

    public IdleState(PlayerCtrl p, StateMachine fsm)
    {
        this.p = p;
        this.fsm = fsm;
    }

    public void Enter() { }

    public void Update()
    {
        if (p.TryConsumeJump())
            fsm.ChangeState(new JumpState(p, fsm));
        else if (!p.IsGrounded())
            fsm.ChangeState(new AirState(p, fsm));
        else if (Mathf.Abs(PlayerInput.Instance.InputX) > 0.01f)
            fsm.ChangeState(new MoveState(p, fsm));
    }

    public void FixedUpdate()
    {
        p.TickMovement(Time.fixedDeltaTime);
        p.TickGravity(Time.fixedDeltaTime);
    }

    public void Exit() { }
}
