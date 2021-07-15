using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using UnityEngine;

public class TraversingState: State, IPropertiesMutable<TraversingState.Properties>
{
    [System.Serializable]
    public struct Properties
    {
        public float climbSpeed;
        public float corneringSpeedMultiplier;
        public bool automaticDetachEnabled;
        public float maxCornerCutTriggerDistance;
    }

    private struct ReflexCorner
    {
        public Vector2 origin;
        public Vector2 corner;

        public Vector2 Vertical => corner * Vector2.up;
        public Vector2 Horizontal => corner * Vector2.right;
    }

    private struct States
    {
        public NormalState normalState;
        public CorneringState corneringState;
    }

    public Event<Vector2> OnWallDetach
    {
        get => stateMachine.States.normalState.OnWallDetach;
        set => stateMachine.States.normalState.OnWallDetach = value;
    }

    public override string Name => $"TraversingState[{stateMachine.CurrentState.Name}]";

    private SpideringInput input;
    private KinematicBody body;
    private Properties properties;

    private StateMachine<States> stateMachine;

    public TraversingState(SpideringInput input, KinematicBody body, Properties properties)
    {
        this.input = input;
        this.body = body;
        this.properties = properties;
        this.stateMachine = new StateMachine<States>(new States
            {
                normalState = new NormalState(this),
                corneringState = new CorneringState(this)
            },
            states =>
            {
                states.corneringState.OnCorneringComplete += () => stateMachine.SetState(states.normalState);
                states.normalState.OnReflexCornerStart += (primaryDir, secondaryDir) =>
                {
                    states.corneringState.PrimaryDirection = primaryDir;
                    states.corneringState.SecondaryDirection = secondaryDir;
                    stateMachine.SetState(states.corneringState);
                };

                return states.normalState;
            }
        );
    }

    public override void OnUpdate(IStateMachine sm)
    {
        stateMachine.CurrentState.Update();
    }

    public void UpdateProperties(PropertiesUpdating<Properties> updater)
    {
        updater?.Invoke(ref properties);
    }

    private class CorneringState: CoroutineState
    {
        public override string Name => "CorneringState";

        public Event OnCorneringComplete;

        public Vector2 PrimaryDirection;
        public Vector2 SecondaryDirection;

        private Vector2 PrimaryDiagonal => (PrimaryDirection + SecondaryDirection).normalized;
        private Vector2 SecondaryDiagonal => (SecondaryDirection - PrimaryDirection).normalized;
        private Vector2 LatchDirection => PrimaryDirection * -1;

        private TraversingState controller;

        public CorneringState(TraversingState controller): base(controller.body)
        {
            this.controller = controller;
        }

        protected override IEnumerator GetCoroutine()
        {
            // Multiply by 1.414 to boost the x or y velocity to equal the normal climbSpeed
            var resolvedCorneringSpeed = controller.properties.climbSpeed * controller.properties.corneringSpeedMultiplier * 1.41421356237f;

            // Left as a variable for debugging
            YieldInstruction routineYield = null;

            // 1. Move into the wall and forward until we're clear of the corner
            while (true)
            {
                var delta = PrimaryDiagonal * resolvedCorneringSpeed * Time.deltaTime;
                controller.body.MoveAndCollide(delta);
                if (!controller.body.CollisionState.HasCollision && CornerCollisionCount(SecondaryDirection) == 0)
                    break;

                yield return routineYield;
            }

            yield return routineYield;

            // 2. Move back into the wall on the other side of the corner
            while (true)
            {
                Debug.DrawRay(controller.body.transform.position, SecondaryDiagonal, Color.yellow);
                var delta = SecondaryDiagonal * resolvedCorneringSpeed * Time.deltaTime;
                controller.body.MoveAndCollide(delta);
                if (controller.body.CollisionState.HasCollision && CornerCollisionCount(LatchDirection) > 0)
                    break;

                yield return routineYield;
            }

            OnCorneringComplete?.Invoke();
        }

        private int CornerCollisionCount(Vector2 direction)
        {
            return controller.body.ExactBounds.GetCorners()
                .Count(c =>
                {
                    var collision = Physics2D.Raycast(c, direction, 0.001f, layerMask: controller.body.CollisionMask);
                    Debug.DrawRay(c, direction * 0.2f, collision ? Color.green : Color.magenta, 1f);
                    return collision;
                });
        }
    }

    private class NormalState : State
    {
        private static float EFFECTIVE_COLLISION_DISTANCE = 0.1f;

        private enum Direction
        {
            UP, DOWN, LEFT, RIGHT
        }

        public override string Name => "TraversingState";

        public Event<Vector2> OnWallDetach;
        public Event<Vector2, Vector2> OnReflexCornerStart;

        private TraversingState controller;

        public NormalState(TraversingState controller)
        {
            this.controller = controller;
        }

        public override void OnUpdate(IStateMachine sm)
        {
            TryMove(new Func<bool>[]{
                TryDetatchFromWall,
                TryCutCorner,
                TryScaleAlongWall,
                TryMoveAroundCorner,
            });
        }

        private bool TryMove(Func<bool>[] moveFns)
        {
            if (controller.input.MovementDirection.sqrMagnitude < 0.1f)
                return false;

            foreach (var fn in moveFns)
            {
                var didMove = fn.Invoke();
                if (didMove)
                    return true;
            }

            return false;
        }

        private bool TryDetatchFromWall()
        {
            if (IsAttemptingToDetatchFromWall() && CanJumpInDirection(controller.input.MovementDirection))
            {
                OnWallDetach?.Invoke(controller.input.MovementDirection);
                return true;
            }

            return false;
        }

        private bool TryMoveAroundCorner()
        {
            if (controller.properties.corneringSpeedMultiplier <= 0)
                return false;

            foreach (var reflexCorner in GetReflexCorners())
            {
                if (CanBendAroundCorner(reflexCorner, out var primaryDirection, out var secondaryDirection) &&
                    ((secondaryDirection * controller.input.MovementDirection).x > 0 || (secondaryDirection * controller.input.MovementDirection).y > 0))
                {
                    Debug.DrawRay(reflexCorner.origin, primaryDirection, Color.yellow);
                    Debug.DrawRay(reflexCorner.origin + primaryDirection, secondaryDirection, Color.cyan);

                    var finalSegmentPosition = reflexCorner.origin + primaryDirection + secondaryDirection;
                    var collisionBackToWall = Physics2D.Raycast(finalSegmentPosition, primaryDirection * -1, primaryDirection.magnitude, layerMask: controller.body.CollisionMask);
                    if (!collisionBackToWall)
                        continue;

                    OnReflexCornerStart?.Invoke(primaryDirection.normalized, secondaryDirection.normalized);
                    return true;
                }
            }

            return false;
        }

        private bool TryCutCorner()
        {
            if (ShouldCutCorner(controller.input.MovementDirection))
            {
                OnWallDetach?.Invoke(controller.input.MovementDirection);
                return true;
            }

            return false;
        }

        private bool TryScaleAlongWall()
        {
            bool didMove = false;
            if (controller.input.MovementDirection.y > 0 && CanScaleInDirection(Vector2.up))
                didMove |= MaybeMoveInDirection(mask: new Vector2(0, 1));
            if (controller.input.MovementDirection.y < 0 && CanScaleInDirection(Vector2.down))
                didMove |= MaybeMoveInDirection(mask: new Vector2(0, 1));
            if (controller.input.MovementDirection.x > 0 && CanScaleInDirection(Vector2.right))
                didMove |= MaybeMoveInDirection(mask: new Vector2(1, 0));
            if (controller.input.MovementDirection.x < 0 && CanScaleInDirection(Vector2.left))
                didMove |= MaybeMoveInDirection(mask: new Vector2(1, 0));

            return didMove;
        }

        private IEnumerable<ReflexCorner> GetReflexCorners()
        {
            foreach (var mc in controller.body.EffectiveBounds.GetMappedCorners())
            {
                yield return new ReflexCorner
                {
                    origin = mc.position,
                    corner = mc.corner
                };
            }
        }

        private bool MaybeMoveInDirection(Vector2 mask)
        {
            var targetDirection = controller.input.MovementDirection * mask;
            if (targetDirection.sqrMagnitude < 0.001f)
                return false;

            var deltaPosition = targetDirection.normalized * Time.deltaTime * controller.properties.climbSpeed;
            controller.body.MoveAndCollide(deltaPosition);
            return deltaPosition.sqrMagnitude > 0f;
        }

        private bool IsAttemptingToDetatchFromWall()
        {
            if (controller.input.IsJumping)
                return true;

            if (!controller.properties.automaticDetachEnabled)
                return false;

            if (IsBodyLatched(Vector2.down) && controller.input.MovementDirection.y > 0.5f)
                return true;
            else if (IsBodyLatched(Vector2.up) && controller.input.MovementDirection.y < -0.5f)
                return true;
            else if (IsBodyLatched(Vector2.left) && controller.input.MovementDirection.x > 0.5f)
                return true;
            else if (IsBodyLatched(Vector2.right) && controller.input.MovementDirection.x < -0.5f)
                return true;

            return false;
        }

        private bool ShouldCutCorner(Vector2 dir)
        {
            if (controller.properties.automaticDetachEnabled)
                return false;

            var heading = dir.normalized;
            var skinOffset = heading * controller.body.SkinWidth * 4;

            bool canSeeWall = controller.body.EffectiveBounds.GetCorners()
                .Any(corner => TestForCollision(corner + skinOffset, heading, controller.properties.maxCornerCutTriggerDistance));

            bool canDetachFromWall = controller.body.EffectiveBounds.GetCorners()
                .All(corner => !TestForCollision(corner + skinOffset, heading, EFFECTIVE_COLLISION_DISTANCE));

            return canSeeWall && canDetachFromWall;
        }

        private bool TestForCollision(Vector2 origin, Vector2 dir, float distance)
            => Physics2D.Raycast(origin, dir, distance, controller.body.CollisionMask);

        private bool CanJumpInDirection(Vector2 dir)
        {
            foreach (var corner in controller.body.EffectiveBounds.GetCorners())
            {
                var origin = corner + (dir.normalized * controller.body.SkinWidth * 4);
                if (Physics2D.Raycast(origin, dir, EFFECTIVE_COLLISION_DISTANCE * 2f, controller.body.CollisionMask))
                    return false;
            }

            return true;
        }

        private bool CanScaleInDirection(Vector2 direction)
        {
            return controller.body.EffectiveBounds.GetMappedCorners()
                .Where(c =>
                {
                    if (direction.x > 0)
                        return c.corner.x > 0;
                    if (direction.x < 0)
                        return c.corner.x < 0;
                    if (direction.y > 0)
                        return c.corner.y > 0;
                    if (direction.y < 0)
                        return c.corner.y < 0;

                    return false;
                })
                .Any(c =>
                {
                    var mask = new Vector2(Mathf.Abs(direction.y), Mathf.Abs(direction.x));
                    var latchDirection = c.corner * mask;
                    return TestForCollision(c.position, latchDirection, EFFECTIVE_COLLISION_DISTANCE);
                });
        }

        private bool CanBendAroundCorner(ReflexCorner corner, out Vector2 primaryDirection, out Vector2 secondaryDirection)
        {
            var reflexTestExtent = 0.2f;
            var origin = corner.origin;
            var dimensions = controller.body.EffectiveBounds.size;
            foreach (var multiplier in new Vector2[] { new Vector2(1, -1), new Vector2(-1, 1) })
            {
                /**
                 * Mapping from the back corner to the diagonal direction
                 * we'd be reflexing around.
                 * 
                 *  1, 1 => [-1, 1  ,  1,-1]
                 * -1,-1 => [-1, 1  ,  1,-1]
                 *  1,-1 => [-1,-1  ,  1, 1]
                 * -1, 1 => [-1,-1  ,  1, 1]
                 */
                var diagonal = corner.corner * multiplier;

                // Attempt to orthogonally step to the diagonal in both possible
                // ways. If we can step in one direction but not the other, we
                // can take this corner
                var horizontal = diagonal.SignedHorizontal();
                var vertical = diagonal.SignedVertical();

                var horizontalSecondaryOffset = horizontal * (dimensions.x + reflexTestExtent);
                var verticalSecondaryOffset = vertical * (dimensions.y + reflexTestExtent);

                var canMoveVertically = !TestForCollision(origin, vertical, verticalSecondaryOffset.magnitude);
                var canMoveHorizontally = !TestForCollision(origin, horizontal, horizontalSecondaryOffset.magnitude);

                if (!canMoveHorizontally && canMoveVertically && !TestForCollision(origin + verticalSecondaryOffset, horizontal, reflexTestExtent))
                {
                    // Ex. Can't move left, but can move down and then left
                    primaryDirection = verticalSecondaryOffset;
                    secondaryDirection = horizontal * reflexTestExtent;
                    return true;
                }
                else if (!canMoveVertically && canMoveHorizontally && !TestForCollision(origin + horizontalSecondaryOffset, vertical, reflexTestExtent))
                {
                    // Ex. Can't move up, but can move right and then up
                    primaryDirection = horizontalSecondaryOffset;
                    secondaryDirection = vertical * reflexTestExtent;
                    return true;
                }
            }

            primaryDirection = default;
            secondaryDirection = default;
            return false;
        }

        private bool IsBodyLatched(Vector2 direction)
        {
            return controller.body.EffectiveBounds.GetCorners()
                .Any(c =>
                {
                    return TestForCollision(c, direction, EFFECTIVE_COLLISION_DISTANCE); ;
                });
        }
    }
}
