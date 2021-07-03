using System;
using UnityEngine;

public abstract class ComponentActivationState: State
{
    public ComponentActivationState()
    {
    }

    protected abstract IActivatable Component { get; }

    public override void OnEnter(IStateMachine sm) => Component.IsActive = true;
    public override void OnExit(IStateMachine sm) => Component.IsActive = false;
}

