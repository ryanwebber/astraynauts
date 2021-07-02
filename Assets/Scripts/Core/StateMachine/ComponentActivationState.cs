using System;
using UnityEngine;

public abstract class ComponentActivationState<TContext>: State<TContext>
{
    public ComponentActivationState()
    {
    }

    protected abstract IActivatable GetComponent(TContext context);

    public override void OnEnter(StateMachine<TContext> sm) => GetComponent(sm.Context).IsActive = true;
    public override void OnExit(StateMachine<TContext> sm) => GetComponent(sm.Context).IsActive = false;
}
