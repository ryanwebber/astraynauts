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

        public States(SpideringInput input, KinematicBody body, Properties properties)
        {
            this.normalState = new NormalState(input, body, properties);
            this.corneringState = new CorneringState(body, properties);
        }
    }

    private StateMachine<States> stateMachine;
    public Event<Vector2> OnWallDetach
    {
        get => stateMachine.States.normalState.OnWallDetach;
        set => stateMachine.States.normalState.OnWallDetach = value;
    }

    public override string Name => $"TraversingState[{stateMachine.CurrentState.Name}]";

    public TraversingState(SpideringInput input, KinematicBody body, Properties properties)
    {
        stateMachine = new StateMachine<States>(new States(input, body, properties), states =>
        {
            states.corneringState.OnCorneringComplete += () => stateMachine.SetState(states.normalState);
            states.normalState.OnReflexCornerStart += (primaryDir, secondaryDir) =>
            {
                states.corneringState.PrimaryDirection = primaryDir;
                states.corneringState.SecondaryDirection = secondaryDir;
                stateMachine.SetState(states.corneringState);
            };

            return states.normalState;
        });
    }

    public override void OnUpdate(IStateMachine sm)
    {
        stateMachine.CurrentState.Update();
    }

    public void UpdateProperties(PropertiesUpdating<Properties> updater)
    {
        stateMachine.States.normalState.UpdateProperties(updater);
    }

    private class CorneringState: CoroutineState, IPropertiesMutable<Properties>
    {
        public override string Name => "CorneringState";

        private Properties properties;
        private KinematicBody body;

        public Event OnCorneringComplete;

        public Vector2 PrimaryDirection;
        public Vector2 SecondaryDirection;

        public Vector2 Diagonal => (PrimaryDirection + SecondaryDirection).normalized;
        public Vector2 SecondaryDiagonal => (SecondaryDirection - PrimaryDirection).normalized;
        public Vector2 LatchDirection => PrimaryDirection * -1;

        public CorneringState(KinematicBody body, Properties properties): base(body)
        {
            this.properties = properties;
            this.body = body;
        }

        protected override IEnumerator GetCoroutine()
        {
            while (true)
            {
                var delta = Diagonal.normalized * properties.climbSpeed * 1.41421356237f * Time.deltaTime;
                body.MoveAndCollide(delta);
                if (!body.CollisionState.HasCollision)
                    break;

                yield return null;
            }

            body.transform.position += ((Vector3)Diagonal).normalized * 0.05f;

            while (CornerCollisionCount(LatchDirection) < 2)
            {
                var delta = SecondaryDiagonal.normalized * properties.climbSpeed * 1.41421356237f * Time.deltaTime;
                body.MoveAndCollide(delta);

                yield return null;
            }

            OnCorneringComplete?.Invoke();
        }

        private int CornerCollisionCount(Vector2 direction)
        {
            return body.EffectiveBounds.GetCorners()
                .Count(c =>
                {
                    var collision = Physics2D.Raycast(c, direction, 0.1f, layerMask: body.CollisionMask);
                    Debug.DrawRay(c, direction * 0.2f, collision ? Color.green : Color.magenta, 1f);
                    return collision;
                });
        }

        public void UpdateProperties(PropertiesUpdating<Properties> updater)
            => updater?.Invoke(ref properties);
    }

    private class NormalState : State, IPropertiesMutable<Properties>
    {
        private static float EFFECTIVE_COLLISION_DISTANCE = 0.1f;

        private enum Direction
        {
            UP, DOWN, LEFT, RIGHT
        }

        public override string Name => "TraversingState";

        public Event<Vector2> OnWallDetach;
        public Event<Vector2, Vector2> OnReflexCornerStart;

        private Properties properties;
        private SpideringInput input;
        private KinematicBody body;

        public NormalState(SpideringInput input, KinematicBody body, Properties properties)
        {
            this.input = input;
            this.body = body;
            this.properties = properties;
        }

        public override void OnUpdate(IStateMachine sm)
        {
            if (IsAttemptingToDetatchFromWall() && CanJumpInDirection(input.MovementDirection))
            {
                OnWallDetach?.Invoke(input.MovementDirection);
                return;
            }

            if (input.MovementDirection.sqrMagnitude < 0.1f)
                return;

            foreach (var reflexCorner in GetReflexCorners())
            {
                if (CanBendAroundCorner(reflexCorner, out var primaryDirection, out var secondaryDirection) &&
                    ((primaryDirection * input.MovementDirection).x > 0 || (primaryDirection * input.MovementDirection).y > 0))
                {
                    Debug.DrawRay(reflexCorner.origin, primaryDirection, Color.yellow);
                    Debug.DrawRay(reflexCorner.origin + primaryDirection, secondaryDirection, Color.cyan);

                    var finalSegmentPosition = reflexCorner.origin + primaryDirection + secondaryDirection;
                    var collisionBackToWall = Physics2D.Raycast(finalSegmentPosition, primaryDirection * -1, primaryDirection.magnitude, layerMask: body.CollisionMask);
                    if (!collisionBackToWall)
                        continue;

                    OnReflexCornerStart?.Invoke(primaryDirection, secondaryDirection);
                    return;
                }
            }

            if (ShouldCutCorner(input.MovementDirection))
            {
                OnWallDetach?.Invoke(input.MovementDirection);
                return;
            }

            if (IsBodyLatched(Direction.LEFT) || IsBodyLatched(Direction.RIGHT))
                MaybeMoveInDirection(mask: new Vector2(0, 1));

            if (IsBodyLatched(Direction.UP) || IsBodyLatched(Direction.DOWN))
                MaybeMoveInDirection(mask: new Vector2(1, 0));
        }

        private IEnumerable<ReflexCorner> GetReflexCorners()
        {
            foreach (var mc in body.EffectiveBounds.GetMappedCorners())
            {
                yield return new ReflexCorner
                {
                    origin = mc.position,
                    corner = mc.corner
                };
            }
        }

        private bool CanBendAroundCorner(ReflexCorner corner, out Vector2 primaryDirection, out Vector2 secondaryDirection)
        {
            var reflexTestRadius = 0.2f;
            var canMoveVertically = !TestForCollision(corner.origin, corner.Vertical, reflexTestRadius);
            var canMoveHorizontally = !TestForCollision(corner.origin, corner.Horizontal, reflexTestRadius);

            if (!canMoveHorizontally && canMoveVertically && !TestForCollision(corner.origin + corner.Vertical * reflexTestRadius, corner.Horizontal, reflexTestRadius))
            {
                // Ex. Can't move left, but can move down and then left
                primaryDirection = corner.Vertical * reflexTestRadius;
                secondaryDirection = corner.Horizontal * reflexTestRadius;
                return true;
            }
            else if (!canMoveVertically && canMoveHorizontally && !TestForCollision(corner.origin + corner.Horizontal * reflexTestRadius, corner.Vertical, reflexTestRadius))
            {
                // Ex. Can't move up, but can move right and then up
                primaryDirection = corner.Horizontal * reflexTestRadius;
                secondaryDirection = corner.Vertical * reflexTestRadius;
                return true;
            }
            else
            {
                primaryDirection = default;
                secondaryDirection = default;
                return false;
            }
        }

        private bool IsAttemptingToDetatchFromWall()
        {
            if (input.IsJumping)
                return true;

            if (!properties.automaticDetachEnabled)
                return false;

            if (IsBodyLatched(Direction.DOWN) && input.MovementDirection.y > 0.5f)
                return true;
            else if (IsBodyLatched(Direction.UP) && input.MovementDirection.y < -0.5f)
                return true;
            else if (IsBodyLatched(Direction.LEFT) && input.MovementDirection.x > 0.5f)
                return true;
            else if (IsBodyLatched(Direction.RIGHT) && input.MovementDirection.x < -0.5f)
                return true;

            return false;
        }

        private bool ShouldCutCorner(Vector2 dir)
        {
            if (properties.automaticDetachEnabled)
                return false;

            var heading = dir.normalized;
            var skinOffset = heading * body.SkinWidth * 4;

            bool canSeeWall = body.EffectiveBounds.GetCorners()
                .Any(corner => TestForCollision(corner + skinOffset, heading, properties.maxCornerCutTriggerDistance));

            bool canDetachFromWall = body.EffectiveBounds.GetCorners()
                .All(corner => !TestForCollision(corner + skinOffset, heading, EFFECTIVE_COLLISION_DISTANCE));

            return canSeeWall && canDetachFromWall;
        }

        private bool TestForCollision(Vector2 origin, Vector2 dir, float distance)
            => Physics2D.Raycast(origin, dir, distance, body.CollisionMask);

        private bool CanJumpInDirection(Vector2 dir)
        {
            foreach (var corner in body.EffectiveBounds.GetCorners())
            {
                var origin = corner + (dir.normalized * body.SkinWidth * 4);
                if (Physics2D.Raycast(origin, dir, EFFECTIVE_COLLISION_DISTANCE * 2f, body.CollisionMask))
                    return false;
            }

            return true;
        }

        private bool MaybeMoveInDirection(Vector2 mask)
        {
            var targetDirection = input.MovementDirection * mask;
            if (targetDirection.sqrMagnitude < 0.001f)
                return false;

            var deltaPosition = targetDirection.normalized * Time.deltaTime * properties.climbSpeed;
            body.MoveAndCollide(deltaPosition);
            return deltaPosition.sqrMagnitude > 0f;
        }

        private bool IsBodyLatched(Direction direction)
        {
            return GetRays(direction, body.EffectiveBounds)
                .All(ray => Physics2D.Raycast(ray.origin, ray.direction, EFFECTIVE_COLLISION_DISTANCE, body.CollisionMask));
        }

        private IEnumerable<Ray2D> GetRays(Direction direction, Bounds bounds)
        {
            switch (direction)
            {
                case Direction.UP:
                    yield return new Ray2D
                    {
                        direction = Vector2.up,
                        origin = bounds.GetTopLeft()
                    };

                    yield return new Ray2D
                    {
                        direction = Vector2.up,
                        origin = bounds.GetTopRight()
                    };

                    break;

                case Direction.DOWN:
                    yield return new Ray2D
                    {
                        direction = Vector2.down,
                        origin = bounds.GetBottomLeft()
                    };

                    yield return new Ray2D
                    {
                        direction = Vector2.down,
                        origin = bounds.GetBottomRight()
                    };

                    break;

                case Direction.RIGHT:
                    yield return new Ray2D
                    {
                        direction = Vector2.right,
                        origin = bounds.GetTopRight()
                    };

                    yield return new Ray2D
                    {
                        direction = Vector2.right,
                        origin = bounds.GetBottomRight()
                    };

                    break;

                case Direction.LEFT:
                    yield return new Ray2D
                    {
                        direction = Vector2.left,
                        origin = bounds.GetBottomLeft()
                    };

                    yield return new Ray2D
                    {
                        direction = Vector2.left,
                        origin = bounds.GetTopLeft()
                    };

                    break;
            }
        }

        public void UpdateProperties(PropertiesUpdating<Properties> updater)
            => updater?.Invoke(ref properties);
    }
}
