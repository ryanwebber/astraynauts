using System;
using UnityEngine;

public class ComponentActivationState: State
{
    private IActivatable activatable;
    public override string Name { get; }

    public ComponentActivationState(IActivatable activatable, string name)
    {
        this.activatable = activatable;
        this.Name = name;
    }

    public sealed override void OnEnter(IStateMachine sm) => activatable.IsActive = true;
    public sealed override void OnExit(IStateMachine sm) => activatable.IsActive = false;
}
