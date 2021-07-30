using System;
using System.Collections.Generic;

public interface IStateMachine
{
    void SetState(State state);
}

public interface IStateHandle
{
    void Update();
    string Name { get; }
}

public abstract class State
{
    public abstract string Name { get; }

    public virtual void OnEnter(IStateMachine sm)
    {
    }

    public virtual void OnUpdate(IStateMachine sm)
    {
    }

    public virtual void OnExit(IStateMachine sm)
    {
    }
}

public class StateMachine<TStates>: IStateMachine
{
    public Event<State, State> OnStateChanged;

    private State currentState = new EmptyState();
    private TStates allStates;

    public TStates States => allStates;
    public IStateHandle CurrentState => new StateHandle
    {
        underlyingState = currentState,
        machine = this
    };

    public StateMachine(TStates allStates, System.Func<TStates, State> initializer = null)
    {
        this.allStates = allStates;
        var initialState = initializer?.Invoke(allStates);
        if (initialState != null)
            SetState(initialState);
    }

    public void SetState(State state)
    {
        var oldState = currentState;
        currentState.OnExit(this);
        currentState = state;
        currentState.OnEnter(this);
        OnStateChanged?.Invoke(oldState, currentState);
    }

    public bool IsStateCurrent(State state)
    {
        return state == currentState;
    }

    public bool IsStateCurrent<T>() where T: State
    {
        return currentState is T;
    }

    private struct StateHandle: IStateHandle
    {
        public State underlyingState;
        public IStateMachine machine;

        public string Name => underlyingState.Name;
        public void Update() => underlyingState.OnUpdate(machine);
    }

    private class EmptyState: State
    {
        public override string Name => "EmptyState";
    }
}
