using System;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Tasks.Actions;
using CleverCrow.Fluid.BTs.Trees;
using UnityEngine;

public class DashBehavior: IBehaviorTreeBuildable
{
    [System.Serializable]
    public struct Properties
    {
        public float dashSpeed;
        public float dashDuration;
        public float jumpHeight;
    }

    public class Input
    {
        public Vector2 Direction { get; set; }
    }

    private class DashAction : ActionBase
    {
        private KinematicBody kinematicBody;
        private Height2D heightComponent;
        private Input input;
        private Properties properties;

        // State
        private float startTime;
        private Vector2 direction;

        public DashAction(KinematicBody kinematicBody, Height2D heightComponent, Input input, Properties properties)
        {
            this.kinematicBody = kinematicBody;
            this.heightComponent = heightComponent;
            this.input = input;
            this.properties = properties;
            this.Name = "Dashing";
        }

        protected override void OnStart()
        {
            startTime = Time.time;
            direction = input.Direction.normalized;
        }

        protected override TaskStatus OnUpdate()
        {
            if (Time.time > startTime + properties.dashDuration)
                return TaskStatus.Success;

            float t = Mathf.Clamp01(Mathf.InverseLerp(startTime, startTime + properties.dashDuration, Time.time));
            heightComponent.Height = PhysicsUtils.LerpGravity(t, properties.jumpHeight);
            kinematicBody.MoveAndCollide(direction * Time.deltaTime * properties.dashSpeed);

            return TaskStatus.Continue;
        }

        protected override void OnExit()
        {
            heightComponent.Height = 0f;
        }
    }

    private KinematicBody kinematicBody;
    private Height2D heightComponent;
    private Input input;
    private Properties properties;

    public DashBehavior(KinematicBody kinematicBody, Height2D heightComponent, Input input, Properties properties)
    {
        this.kinematicBody = kinematicBody;
        this.heightComponent = heightComponent;
        this.input = input;
        this.properties = properties;
    }

    public BehaviorTree ToBehaviorTree(GameObject obj)
    {
        return new BehaviorTreeBuilder(obj)
            .AddNode(new DashAction(kinematicBody, heightComponent, input, properties))
            .Build();
    }
}
