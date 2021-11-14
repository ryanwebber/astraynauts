using System;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;
using UnityEngine;

public class SingleShotBehavior : IBehaviorTreeBuildable
{
    [System.Serializable]
    public struct Properties
    {
        public float cooldownTime;
    }

    public class Input
    {
        public Vector2 Aim { get; set; }
        public bool IsFiring;
    }

    public Event<Vector2> OnFireShot;

    private Input input;
    private Properties properties;

    public SingleShotBehavior(Input input, Properties properties)
    {
        this.input = input;
        this.properties = properties;
    }

    public BehaviorTree ToBehaviorTree(GameObject obj)
    {
        return new BehaviorTreeBuilder(obj)
            .Sequence()
                .Condition(() => input.IsFiring)
                .Do(() =>
                {
                    OnFireShot?.Invoke(input.Aim);
                    return TaskStatus.Success;
                })
                .WaitTime(properties.cooldownTime)
            .End()
            .Build();
    }
}
