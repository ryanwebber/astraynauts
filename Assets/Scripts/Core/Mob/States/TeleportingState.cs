﻿using System;

public class TeleportingState : State
{
    public override string Name => "TeleportingState";

    private HealthManager healthManager;

    public TeleportingState(HealthManager healthManager)
    {
        this.healthManager = healthManager;
    }

    public override void OnEnter(IStateMachine sm)
    {
        healthManager.SetState(HealthManager.Damagability.TRANSPARENT);
    }

    public override void OnExit(IStateMachine sm)
    {
        healthManager.SetState(HealthManager.Damagability.VULNERABLE);
    }
}