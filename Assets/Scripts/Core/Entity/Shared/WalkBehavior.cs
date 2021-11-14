using System;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Tasks.Actions;
using CleverCrow.Fluid.BTs.Trees;
using UnityEngine;

public class WalkBehavior: IBehaviorTreeBuildable
{
    [System.Serializable]
    public struct Properties
    {
        public float movementSpeed;
        public float movementDampening;
    }

    public class Input
    {
        public Vector2 Direction { get; set; }
    }

    private class WalkAction : ActionBase
    {
        private KinematicBody kinematicBody;
        private Input input;
        private Properties properties;

        // State
        private Vector2 velocity;
        public Vector2 Velocity
        {
            get => velocity;
            set => velocity = value;
        }

        public Vector2 Heading { get; private set; }

        public WalkAction(KinematicBody kinematicBody, Input input, Properties properties)
        {
            this.kinematicBody = kinematicBody;
            this.input = input;
            this.properties = properties;
            this.Name = "Walking";
        }

        protected override TaskStatus OnUpdate()
        {
            var currentHeading = Heading;
            var targetHeading = Vector2.ClampMagnitude(input.Direction, 1f);

            var dampenedHeading = Vector2.SmoothDamp(
                current: currentHeading,
                target: targetHeading,
                currentVelocity: ref velocity,
                smoothTime: properties.movementDampening);

            kinematicBody.MoveAndCollide(dampenedHeading * Time.deltaTime * properties.movementSpeed);

            Heading = dampenedHeading;

            return TaskStatus.Success;
        }
    }

    private KinematicBody kinematicBody;
    private Input input;
    private Properties properties;

    public WalkBehavior(KinematicBody kinematicBody, Input input, Properties properties)
    {
        this.kinematicBody = kinematicBody;
        this.input = input;
        this.properties = properties;
    }

    public BehaviorTree ToBehaviorTree(GameObject obj)
    {
        return new BehaviorTreeBuilder(obj)
            .AddNode(new WalkAction(kinematicBody, input, properties))
            .Build();
    }
}
