using System;
using UnityEngine;

public class ComponentActivationState<T>: State where T: IActivatable
{
    private T activatable;
    private Action<T> resetFn;

    public T Component => activatable;
    public override string Name { get; }

    public ComponentActivationState(T activatable, string name, Action<T> resetFn = null)
    {
        this.resetFn = resetFn;
        this.activatable = activatable;
        this.Name = name;
    }

    public sealed override void OnEnter(IStateMachine sm)
    {
        resetFn?.Invoke(activatable);
        activatable.IsActive = true;
    }

    public sealed override void OnExit(IStateMachine sm) => activatable.IsActive = false;
}
