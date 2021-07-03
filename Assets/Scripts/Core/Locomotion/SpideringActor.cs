using Extensions;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(KinematicBody))]
[RequireComponent(typeof(SpideringInput))]
public class SpideringActor : MonoBehaviour, IActivatable
{
    private struct States
    {
        public FloatState floatState;
        public ClimbState climbState;
    }

    [SerializeField]
    private FloatState.Properties floatStateProperties;

    [SerializeField]
    private ClimbState.Properties climbStateProperties;

    [SerializeField]
    private bool isActive;
    public bool IsActive
    {
        get => isActive;
        set => isActive = value;
    }

    private StateMachine<States> stateMachine;

    private void Awake()
    {
        var kinematicBody = GetComponent<KinematicBody>();
        var virtualInput = GetComponent<SpideringInput>();

        var states = new States
        {
            floatState = new FloatState(virtualInput, kinematicBody, floatStateProperties),
            climbState = new ClimbState(virtualInput, kinematicBody, climbStateProperties),
        };
        
        stateMachine = new StateMachine<States>(states, states.floatState);
    }

    private void Update()
    {
        stateMachine.CurrentState?.Update();
    }

    private class FloatState : State<States>
    {
        [System.Serializable]
        public struct Properties
        {
            public float jumpForce;
            public float steerForce;
            public float maxVelocity;
        }

        public override string Name => "FloatState";

        private SpideringInput input;
        private KinematicBody body;
        private Properties properties;

        private Vector2 velocity;
        public Vector2 JumpHeading
        {
            set => velocity = value.normalized * properties.jumpForce;
        }

        public FloatState(SpideringInput input, KinematicBody body, Properties properties)
        {
            this.input = input;
            this.body = body;
            this.properties = properties;
            this.velocity = Vector2.zero;
        }

        public override void OnUpdate(StateMachine<States> sm)
        {
            velocity = Vector2.ClampMagnitude(velocity + input.MovementDirection * properties.steerForce, properties.maxVelocity);

            body.MoveAndCollide(velocity * Time.deltaTime);

            if (body.CollisionState.HasCollision)
                sm.SetState(sm.Context.climbState);
        }
    }

    private class ClimbState: State<States>
    {
        [System.Serializable]
        public struct Properties
        {
            public float climbSpeed;
        }

        private static float EFFECTIVE_COLLISION_DISTANCE = 0.1f;

        private enum Direction
        {
            UP, DOWN, LEFT, RIGHT
        }

        public override string Name => "ClimbState";

        private SpideringInput input;
        private KinematicBody body;
        private Properties properties;

        public ClimbState(SpideringInput input, KinematicBody body, Properties properties)
        {
            this.input = input;
            this.body = body;
            this.properties = properties;
        }

        public override void OnEnter(StateMachine<States> sm)
        {
            Debug.Log("Is climbing!");
        }

        public override void OnUpdate(StateMachine<States> sm)
        {
            if (input.IsJumping && CanJumpInDirection(input.MovementDirection))
            {
                sm.Context.floatState.JumpHeading = input.MovementDirection;
                sm.SetState(sm.Context.floatState);
                return;
            }


            var xTranslation = Mathf.Abs(input.MovementDirection.x);
            var yTranslation = Mathf.Abs(input.MovementDirection.y);

            if (yTranslation >= xTranslation)
                if (IsBodyLatched(body, Direction.LEFT) || IsBodyLatched(body, Direction.RIGHT))
                    MaybeMoveInDirection(mask: new Vector2(0, 1));

            if (xTranslation >= yTranslation)
                if (IsBodyLatched(body, Direction.UP) || IsBodyLatched(body, Direction.DOWN))
                    MaybeMoveInDirection(mask: new Vector2(1, 0));
        }

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
            var deltaPosition = input.MovementDirection * mask * Time.deltaTime * properties.climbSpeed;
            body.MoveAndCollide(deltaPosition);
            return deltaPosition.sqrMagnitude > 0f;
        }

        private bool IsBodyLatched(KinematicBody body, Direction direction)
        {
            return GetRays(direction, body.EffectiveBounds)
                .All(ray => {
                    Debug.DrawRay(ray.origin, ray.direction, Color.cyan);
                    return Physics2D.Raycast(ray.origin, ray.direction, EFFECTIVE_COLLISION_DISTANCE, body.CollisionMask);
                });
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
    }
}
