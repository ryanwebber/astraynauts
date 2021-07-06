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
                var delta = Diagonal * properties.climbSpeed * 1.41421356237f * Time.deltaTime;
                body.MoveAndCollide(delta);
                if (!body.CollisionState.HasCollision)
                    break;

                yield return null;
            }

            body.transform.position += ((Vector3)Diagonal).normalized * 0.05f;

            while (CornerCollisionCount(LatchDirection) < 2)
            {
                var delta = SecondaryDiagonal * properties.climbSpeed * 1.41421356237f * Time.deltaTime;
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
            TryMove(new Func<bool>[]{
                TryDetatchFromWall,
                TryCutCorner,
                TryScaleAlongWall,
                TryMoveAroundCorner,
            });
        }

        private bool TryMove(Func<bool>[] moveFns)
        {
            if (input.MovementDirection.sqrMagnitude < 0.1f)
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
            if (IsAttemptingToDetatchFromWall() && CanJumpInDirection(input.MovementDirection))
            {
                OnWallDetach?.Invoke(input.MovementDirection);
                return true;
            }

            return false;
        }

        private bool TryMoveAroundCorner()
        {
            foreach (var reflexCorner in GetReflexCorners())
            {
                if (CanBendAroundCorner(reflexCorner, out var primaryDirection, out var secondaryDirection) &&
                    ((secondaryDirection * input.MovementDirection).x > 0 || (secondaryDirection * input.MovementDirection).y > 0))
                {
                    Debug.DrawRay(reflexCorner.origin, primaryDirection, Color.yellow);
                    Debug.DrawRay(reflexCorner.origin + primaryDirection, secondaryDirection, Color.cyan);

                    var finalSegmentPosition = reflexCorner.origin + primaryDirection + secondaryDirection;
                    var collisionBackToWall = Physics2D.Raycast(finalSegmentPosition, primaryDirection * -1, primaryDirection.magnitude, layerMask: body.CollisionMask);
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
            if (ShouldCutCorner(input.MovementDirection))
            {
                OnWallDetach?.Invoke(input.MovementDirection);
                return true;
            }

            return false;
        }

        private bool TryScaleAlongWall()
        {
            bool didMove = false;
            if (input.MovementDirection.y > 0 && CanScaleInDirection(Vector2.up))
                didMove |= MaybeMoveInDirection(mask: new Vector2(0, 1));
            if (input.MovementDirection.y < 0 && CanScaleInDirection(Vector2.down))
                didMove |= MaybeMoveInDirection(mask: new Vector2(0, 1));
            if (input.MovementDirection.x > 0 && CanScaleInDirection(Vector2.right))
                didMove |= MaybeMoveInDirection(mask: new Vector2(1, 0));
            if (input.MovementDirection.x < 0 && CanScaleInDirection(Vector2.left))
                didMove |= MaybeMoveInDirection(mask: new Vector2(1, 0));

            return didMove;
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

        private bool MaybeMoveInDirection(Vector2 mask)
        {
            var targetDirection = input.MovementDirection * mask;
            if (targetDirection.sqrMagnitude < 0.001f)
                return false;

            var deltaPosition = targetDirection.normalized * Time.deltaTime * properties.climbSpeed;
            body.MoveAndCollide(deltaPosition);
            return deltaPosition.sqrMagnitude > 0f;
        }

        private bool IsAttemptingToDetatchFromWall()
        {
            if (input.IsJumping)
                return true;

            if (!properties.automaticDetachEnabled)
                return false;

            if (IsBodyLatched(Vector2.down) && input.MovementDirection.y > 0.5f)
                return true;
            else if (IsBodyLatched(Vector2.up) && input.MovementDirection.y < -0.5f)
                return true;
            else if (IsBodyLatched(Vector2.left) && input.MovementDirection.x > 0.5f)
                return true;
            else if (IsBodyLatched(Vector2.right) && input.MovementDirection.x < -0.5f)
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

        private bool CanScaleInDirection(Vector2 direction)
        {
            return body.EffectiveBounds.GetMappedCorners()
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
            var dimensions = body.EffectiveBounds.size;
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
            return body.EffectiveBounds.GetCorners()
                .Any(c =>
                {
                    return TestForCollision(c, direction, EFFECTIVE_COLLISION_DISTANCE); ;
                });
        }

        public void UpdateProperties(PropertiesUpdating<Properties> updater)
            => updater?.Invoke(ref properties);
    }
}
