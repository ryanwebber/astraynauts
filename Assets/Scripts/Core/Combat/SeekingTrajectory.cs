using Extensions;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Projectile))]
public class SeekingTrajectory : MonoBehaviour
{
    private struct States
    {
        public SeekingState seekingState;
        public TrackingState trackingState;

        public static States FromController(SeekingTrajectory controller)
        {
            return new States
            {
                seekingState = new SeekingState(controller),
                trackingState = new TrackingState(controller),
            };
        }
    }

    [SerializeField]
    [Min(0)]
    private float maxSteerForce = 0.2f;

    [SerializeField]
    [Min(0)]
    private float steerForceMultiplier = 1;

    [SerializeField]
    [Min(0)]
    private float maxSeekVision = 2f;

    [SerializeField]
    private float maxTrackTime = 0.25f;

    [SerializeField]
    private LayerMask targetMask;

    [SerializeField]
    private LayerMask obstructionMask;

    [SerializeField]
    [Min(1)]
    private int maxTargetSortCount = 4;

    private Velocity2D velocityComponent;
    private StateMachine<States> stateMachine;

    private Vector2 Heading
    {
        get => velocityComponent.CurrentVelocity;
        set => velocityComponent.CurrentVelocity = value;
    }

    private void Awake()
    {
        velocityComponent = GetComponent<Velocity2D>();
    }

    private void Start()
    {
        StartCoroutine(Coroutines.After(2f, () => Destroy(gameObject)));
        stateMachine = new StateMachine<States>(States.FromController(this), states =>
        {
            states.seekingState.OnTargetLock += target =>
            {
                states.trackingState.Target = target;
                stateMachine.SetState(states.trackingState);
            };

            states.trackingState.OnTargetLost += () =>
            {
                stateMachine.SetState(states.seekingState);
            };

            return states.seekingState;
        });
    }

    private void Update()
    {
        stateMachine.CurrentState.Update();
    }

    private class SeekingState: State
    {
        public Event<Transform> OnTargetLock;

        private SeekingTrajectory controller;
        private Collider2D[] reusableCastResults;

        private Vector2 Position => controller.transform.position;
        private Vector2 Heading => controller.Heading;

        public override string Name => "DefaultState";

        public SeekingState(SeekingTrajectory controller)
        {
            this.controller = controller;
            this.reusableCastResults = new Collider2D[controller.maxTargetSortCount];
        }

        public override void OnUpdate(IStateMachine sm)
        {
            var collisionCount = Physics2D.OverlapCircleNonAlloc(Position, controller.maxSeekVision, reusableCastResults, controller.targetMask);
            var bestTarget = reusableCastResults.Take(collisionCount)
                .Where(c => IsTargetable(c.transform.position))
                .OrderBy(c => Vector2.Distance(Position, c.transform.position))
                .FirstOrDefault();

            if (bestTarget != null)
            {
                OnTargetLock?.Invoke(bestTarget.transform);
            }
        }

        private bool IsTargetable(Vector2 target)
        {
            Vector2 position = Position;
            bool IsNearby() => Vector2.Distance(position, target) <= controller.maxSeekVision;
            bool IsSteerable() => GetSteerExclusionZones(Heading).All(z => !z.ContainsPoint(target));
            bool IsForwards() => Vector2.Dot(Heading, (target - position)) > 0f;
            bool IsVisible()
            {
                // Cast a ray directly to target position. We should hit no obstructions
                return !Physics2D.Raycast(position, (target - position), Vector2.Distance(target, position), layerMask: controller.obstructionMask);
            };

            return IsNearby() && IsForwards() && IsSteerable() && IsVisible();
        }

        private IEnumerable<Circle> GetSteerExclusionZones(Vector2 usingHeading)
        {
            foreach (var sign in new int[] { 1, -1 })
            {
                GetSteerPath(usingHeading, sign, out var p1, out var p2, out var p3);
                if (Circle.FromPointsOnCircumference(p1, p2, p3, out var circle))
                    yield return circle;
            }
        }

        private void GetSteerPath(Vector2 inHeading, float turnDirection, out Vector2 p1, out Vector2 p2, out Vector3 p3)
        {
            var sign = Mathf.Sign(turnDirection);
            Vector2 v1 = inHeading;
            p1 = Position;

            Vector2 v2 = v1.Rotate(controller.maxSteerForce * sign);
            p2 = p1 + v2;

            Vector2 v3 = v2.Rotate(controller.maxSteerForce * sign);
            p3 = p2 + v3;
        }
    }

    private class TrackingState: State
    {
        public Event OnTargetLost;

        private float startTime;
        private SeekingTrajectory controller;

        private Vector2 Position => controller.transform.position;
        private Vector2 Heading
        {
            get => controller.Heading;
            set => controller.Heading = value;
        }

        public Transform Target;

        public override string Name => "TrackingState";

        public TrackingState(SeekingTrajectory controller)
        {
            this.controller = controller;

            // Don't hold onto old transform references
            OnTargetLost += () => Target = null;
        }

        public override void OnEnter(IStateMachine sm)
        {
            startTime = Time.time;
        }

        public override void OnUpdate(IStateMachine sm)
        {
            if (Target == null)
            {
                AbandonTarget();
                return;
            }

            if (Time.time > startTime + controller.maxTrackTime)
            {
                AbandonTarget();
                return;
            }

            Vector2 targetPosition = Target.transform.position;
            Vector2 currentPosition = Position;
            Vector2 targetHeading = targetPosition - currentPosition;

            Debug.DrawLine(currentPosition, targetPosition, Color.green);

            // If the target is now behind us, exit out of tracking
            if (Vector2.Dot(Heading, targetHeading) < 0f)
            {
                AbandonTarget();
                return;
            }

            Vector2 newHeading = Vector3.RotateTowards(Heading, targetHeading, controller.maxSteerForce * Time.deltaTime * controller.steerForceMultiplier, 0f);
            Heading = newHeading;
        }

        private void AbandonTarget()
        {
            OnTargetLost?.Invoke();
        }
    }
}
