using System;
using System.Collections.Generic;

public abstract class BaseStateBuilder<T>
{
    protected abstract T Self { get; }

    public abstract T OnEnter(Action action);
    public abstract T OnExit(Action action);
    public abstract T OnUpdate(Action action);

    public T Bind(IActivatable activatable)
    {
        OnEnter(() => activatable.IsActive = true);
        OnExit(() => activatable.IsActive = false);
        return Self;
    }
}

public class StateBuilder: BaseStateBuilder<StateBuilder>
{
    private string name;
    private List<Action> onEnterActions = new List<Action>();
    private List<Action> onExitActions = new List<Action>();
    private List<Action> onUpdateActions = new List<Action>();

    protected override StateBuilder Self => this;

    public StateBuilder(string name)
    {
        this.name = name;
    }

    public override StateBuilder OnEnter(Action action)
    {
        onEnterActions.Add(action);
        return this;
    }

    public override StateBuilder OnExit(Action action)
    {
        onExitActions.Add(action);
        return this;
    }

    public override StateBuilder OnUpdate(Action action)
    {
        onUpdateActions.Add(action);
        return this;
    }

    public IStateRunnable Build()
    {
        return new ParameterizedState(
            name: this.name,
            onEnter: () => onEnterActions.ForEach(action => action?.Invoke()),
            onExit: () => onExitActions.ForEach(action => action?.Invoke()),
            onUpdate: () => onUpdateActions.ForEach(action => action?.Invoke())
        );
    }
}

public class StateBuilder<T1>: BaseStateBuilder<StateBuilder<T1>>
{
    private string name;
    private List<Action<T1>> onEnterActions = new List<Action<T1>>();
    private List<Action<T1>> onExitActions = new List<Action<T1>>();
    private List<Action<T1>> onUpdateActions = new List<Action<T1>>();

    protected override StateBuilder<T1> Self => this;

    public StateBuilder(string name)
    {
        this.name = name;
    }

    public override StateBuilder<T1> OnEnter(Action action)
    {
        onEnterActions.Add((_1) => action?.Invoke());
        return this;
    }

    public override StateBuilder<T1> OnExit(Action action)
    {
        onExitActions.Add((_1) => action?.Invoke());
        return this;
    }

    public override StateBuilder<T1> OnUpdate(Action action)
    {
        onUpdateActions.Add((_1) => action?.Invoke());
        return this;
    }

    public StateBuilder<T1> OnEnter(Action<T1> action)
    {
        onEnterActions.Add(action);
        return this;
    }

    public StateBuilder<T1> OnExit(Action<T1> action)
    {
        onExitActions.Add(action);
        return this;
    }

    public StateBuilder<T1> OnUpdate(Action<T1> action)
    {
        onUpdateActions.Add(action);
        return this;
    }

    public IStateRunnable Build(T1 arg1)
    {
        return new ParameterizedState(
            name: this.name,
            onEnter: () => onEnterActions.ForEach(action => action?.Invoke(arg1)),
            onExit: () => onExitActions.ForEach(action => action?.Invoke(arg1)),
            onUpdate: () => onUpdateActions.ForEach(action => action?.Invoke(arg1))
        );
    }
}

public class StateBuilder<T1, T2>: BaseStateBuilder<StateBuilder<T1, T2>>
{
    private string name;
    private List<Action<T1, T2>> onEnterActions = new List<Action<T1, T2>>();
    private List<Action<T1, T2>> onExitActions = new List<Action<T1, T2>>();
    private List<Action<T1, T2>> onUpdateActions = new List<Action<T1, T2>>();

    protected override StateBuilder<T1, T2> Self => this;

    public StateBuilder(string name)
    {
        this.name = name;
    }

    public override StateBuilder<T1, T2> OnEnter(Action action)
    {
        onEnterActions.Add((_1, _2) => action?.Invoke());
        return this;
    }

    public override StateBuilder<T1, T2> OnExit(Action action)
    {
        onExitActions.Add((_1, _2) => action?.Invoke());
        return this;
    }

    public override StateBuilder<T1, T2> OnUpdate(Action action)
    {
        onUpdateActions.Add((_1, _2) => action?.Invoke());
        return this;
    }

    public StateBuilder<T1, T2> OnEnter(Action<T1, T2> action)
    {
        onEnterActions.Add(action);
        return this;
    }

    public StateBuilder<T1, T2> OnExit(Action<T1, T2> action)
    {
        onExitActions.Add(action);
        return this;
    }

    public StateBuilder<T1, T2> OnUpdate(Action<T1, T2> action)
    {
        onUpdateActions.Add(action);
        return this;
    }

    public IStateRunnable Build(T1 arg1, T2 arg2)
    {
        return new ParameterizedState(
            name: this.name,
            onEnter: () => onEnterActions.ForEach(action => action?.Invoke(arg1, arg2)),
            onExit: () => onExitActions.ForEach(action => action?.Invoke(arg1, arg2)),
            onUpdate: () => onUpdateActions.ForEach(action => action?.Invoke(arg1, arg2))
        );
    }
}
