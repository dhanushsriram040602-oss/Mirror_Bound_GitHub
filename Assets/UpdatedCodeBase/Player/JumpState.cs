using UnityEngine;

public class JumpState : IState
{
    PlayerCtrl p;
    StateMachine fsm;

    public JumpState(PlayerCtrl p, StateMachine fsm)
    {
        this.p = p;
        this.fsm = fsm;
    }

    public void Enter()
    {
        p.ExecuteJump();
    }

    public void Update()
    {
        fsm.ChangeState(new AirState(p, fsm));
    }

    public void FixedUpdate()
    {
        p.TickMovement(Time.fixedDeltaTime);
        p.TickGravity(Time.fixedDeltaTime);
    }

    public void Exit() { }
}
