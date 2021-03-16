using System;
using System.Collections.Generic;

public class StateMachine
{
    private IStateRunnable state;

    private StateMachine()
    {
        this.state = null;
    }

    private void SetState(IStateRunnable state)
    {
        this.state?.OnExit();
        this.state = state;
        this.state?.OnEnter();
    }

    public static StateMachine In(System.Action<Scope> scope)
    {
        var stateMachine = new StateMachine();
        var builder = new Scope(stateMachine);
        scope?.Invoke(builder);

        return stateMachine;
    }

    public class Scope
    {
        private StateMachine stateMachine;

        public Scope(StateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        public void Enter(IStateRunnable state)
        {
            stateMachine.SetState(state);
        }
    }
}
