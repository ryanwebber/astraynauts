using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashInDirectionState : State
{
    public override string Name => "DashState";
    public Vector2 Direction { get; set; }

    private DashActor actor;
    private Func<State> nextStateProvider;

    public DashInDirectionState(DashActor actor, Func<State> nextStateProvider)
    {
        this.actor = actor;
        this.nextStateProvider = nextStateProvider;
    }

    public DashInDirectionState(DashActor actor, State nextState)
    {
        this.actor = actor;
        this.nextStateProvider = () => nextState;
    }

    public override void OnEnter(IStateMachine sm)
    {
        void HandleDashEnd()
        {
            actor.OnDashEnd -= HandleDashEnd;

            if (actor.Behavior.IsCurrent)
                sm.SetState(nextStateProvider());
        }

        actor.OnDashEnd += HandleDashEnd;
        actor.DashInDirection(Direction);
    }
}
