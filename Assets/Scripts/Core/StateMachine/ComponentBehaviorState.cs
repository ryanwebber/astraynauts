using UnityEngine;
using System.Collections;
using System;

public class ComponentBehaviorState : State
{
    public override string Name => name;

    private string name;
    private ComponentBehavior behavior;

    public ComponentBehaviorState(ComponentBehavior behavior)
    {
        this.behavior = behavior;
        this.name = $"{behavior.name}State";
    }

    public ComponentBehaviorState(ComponentBehavior behavior, string name)
    {
        this.behavior = behavior;
        this.name = name;
    }

    public override void OnEnter(IStateMachine sm)
    {
        behavior.MakeCurrent();
    }

    public override void OnExit(IStateMachine sm)
    {
        behavior.ExitIfCurrent();
    }
}

public interface BehaviorControlling
{
    public ComponentBehavior Behavior { get; }
}

public class ComponentBehaviorState<T>: State where T: BehaviorControlling
{
    public override string Name => name;

    private string name;
    private T controller;
    private Action<T, LifecycleEvent> resetFn;

    public ComponentBehaviorState(T controller, string name, Action<T, LifecycleEvent> resetFn)
    {
        this.controller = controller;
        this.name = name;
        this.resetFn = resetFn;
    }

    public sealed override void OnEnter(IStateMachine sm)
    {
        resetFn?.Invoke(controller, LifecycleEvent.BEGIN);
        controller.Behavior.MakeCurrent();
    }

    public sealed override void OnExit(IStateMachine sm)
    {
        resetFn?.Invoke(controller, LifecycleEvent.END);
        controller.Behavior.ExitIfCurrent();
    }
}
