using System;
using UnityEngine;

public class ComponentActivationState<T>: State where T: IActivatable
{
    private T activatable;
    private Action<T, LifecycleEvent> resetFn;

    public T Component => activatable;
    public override string Name { get; }

    public ComponentActivationState(T activatable, string name, Action<T, LifecycleEvent> resetFn = null)
    {
        this.resetFn = resetFn;
        this.activatable = activatable;
        this.Name = name;
    }

    public sealed override void OnEnter(IStateMachine sm)
    {
        resetFn?.Invoke(activatable, LifecycleEvent.BEGIN);
        activatable.IsActive = true;
    }

    public sealed override void OnExit(IStateMachine sm)
    {
        resetFn?.Invoke(activatable, LifecycleEvent.END);
        activatable.IsActive = false;
    }
}
