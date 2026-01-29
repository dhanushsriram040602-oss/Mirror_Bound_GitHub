using UnityEngine;

public class AirState : IState
{
    PlayerCtrl p;
    StateMachine fsm;

    public AirState(PlayerCtrl p, StateMachine fsm)
    {
        this.p = p;
        this.fsm = fsm;
    }

    public void Enter() { }

    public void Update()
    {
        if (p.IsGrounded())
        {
            if (Mathf.Abs(PlayerInput.Instance.InputX) > 0.01f)
                fsm.ChangeState(new MoveState(p, fsm));
            else
                fsm.ChangeState(new IdleState(p, fsm));
        }
    }

    public void FixedUpdate()
    {
        p.TickMovement(Time.fixedDeltaTime);
        p.TickJumpHold(Time.fixedDeltaTime);
        p.TickGravity(Time.fixedDeltaTime);
    }


    public void Exit() { }
}
