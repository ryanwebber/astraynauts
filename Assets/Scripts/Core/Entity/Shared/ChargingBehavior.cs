using System;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;
using UnityEngine;

public class ChargingBehavior : IBehaviorTreeBuildable
{
    [System.Serializable]
    public struct Properties
    {
        public float chargeStepTime;
    }

    public Event OnChargingStep;

    private Properties properties;

    public ChargingBehavior(Properties properties)
    {
        this.properties = properties;
    }

    public BehaviorTree ToBehaviorTree(GameObject obj)
    {
        return new BehaviorTreeBuilder(obj)
            .RepeatForever("Charge loop")
                .WaitTime(1f)
                .Do(() =>
                {
                    OnChargingStep?.Invoke();
                    return TaskStatus.Success;
                })
            .End()
            .Build();
    }
}
