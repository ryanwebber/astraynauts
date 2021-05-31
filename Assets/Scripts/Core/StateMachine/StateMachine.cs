using System;
using System.Collections.Generic;

public abstract class State<T>
{
    public abstract string Name { get; }

    public virtual void OnEnter(StateMachine<T> sm)
    {
    }

    public virtual void OnUpdate(StateMachine<T> sm)
    {
    }

    public virtual void OnExit(StateMachine<T> sm)
    {
    }
}

public interface IStateHandle
{
    void Update();
}

public class StateMachine<T>
{
    public Event<State<T>, State<T>> OnStateChanged;

    private T context;
    private State<T> currentState = new EmptyState<T>();

    public IStateHandle CurrentState => new StateHandle
    {
        underlyingState = currentState,
        machine = this
    };

    public T Context => context;

    public  StateMachine(T context, State<T> initialState = null)
    {
        this.context = context;
        if (initialState != null)
            SetState(initialState);
    }

    public void SetState(State<T> state)
    {
        var oldState = currentState;
        currentState.OnExit(this);
        currentState = state;
        currentState.OnEnter(this);
        OnStateChanged?.Invoke(oldState, currentState);
    }

    private struct StateHandle: IStateHandle
    {
        public State<T> underlyingState;
        public StateMachine<T> machine;

        public void Update() => underlyingState.OnUpdate(machine);
    }

    private class EmptyState<TContext>: State<TContext>
    {
        public override string Name => "EmptyState";
    }
}
