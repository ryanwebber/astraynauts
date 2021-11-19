using System;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Tasks.Actions;
using CleverCrow.Fluid.BTs.Trees;
using UnityEngine;

public class GridStepBehavior : IBehaviorTreeBuildable
{
    public class Input
    {
        public Vector2 Direction { get; set; }
    }

    private class GridStepAction : ActionBase
    {
        private GridLockedBody body;
        private Input input;

        public GridStepAction(GridLockedBody body, Input input)
        {
            this.body = body;
            this.input = input;
            this.Name = "GridTraversial";
        }

        protected override TaskStatus OnUpdate()
        {
            if (body.IsMoving)
                return TaskStatus.Failure;

            if (input.Direction.sqrMagnitude < 0.25f)
                return TaskStatus.Failure;

            float theta = Vector2.SignedAngle(Vector2.right, input.Direction);
            if (Mathf.Abs(theta % 45) < 5f)
            {
                // We're essentially going diagonally, choose a direction randomly
                theta += UnityEngine.Random.value < 0.5f ? -6f : 6f;
            }

            Vector2Int offset;
            switch (theta)
            {
                case float a when (a > 135):
                    offset = Vector2Int.left;
                    break;
                case float a when (a > 45):
                    offset = Vector2Int.up;
                    break;
                case float a when (a > -45):
                    offset = Vector2Int.right;
                    break;
                case float a when (a > -135):
                    offset = Vector2Int.down;
                    break;
                default:
                    offset = Vector2Int.left;
                    break;
            }

            body.MoveBy(offset);

            return TaskStatus.Success;
        }
    }

    private GridLockedBody body;
    private Input input;
    private float movementDelay;

    public GridStepBehavior(GridLockedBody body, Input input, float movementDelay)
    {
        this.body = body;
        this.input = input;
        this.movementDelay = movementDelay;
    }

    public BehaviorTree ToBehaviorTree(GameObject obj)
    {
        return new BehaviorTreeBuilder(obj)
            .ReturnSuccess()
                .Sequence()
                    .AddNode(new GridStepAction(body, input))
                    .WaitUntil(() => !body.IsMoving)
                    .WaitTime(movementDelay)
                .End()
            .End()
            .Build();
    }
}
