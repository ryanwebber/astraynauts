using UnityEngine;
using System.Collections;
using System.Threading;

[RequireComponent(typeof(DashActor))]
[RequireComponent(typeof(WalkingActor))]
public class PlayerMovementController : MonoBehaviour
{
    public Event OnDashInputBegin;
    public Event OnDashStateEnter;
    public Event OnDashStateExit;
    public Event OnFreeRunStateEnter;
    public Event OnFreeRunStateExit;

    private struct States
    {
        public State runState;
        public DashState dashState;
        public EmptyState idleState;

        public static States FromComponent(PlayerMovementController controller)
        {
            return new States
            {
                runState = new ComponentActivationState<WalkingActor>(controller.walkingActor, "RunningState", (actor, state) =>
                {
                    actor.EraseMomentum();

                    if (state == LifecycleEvent.BEGIN)
                        controller.OnFreeRunStateEnter?.Invoke();
                    else if (state == LifecycleEvent.END)
                        controller.OnFreeRunStateExit?.Invoke();
                }),
                dashState = new DashState(controller),
                idleState = new EmptyState("IdleState"),
            };
        }

        public State DefaultMovementState => runState;
    }

    [SerializeField]
    private BatteryManager batteryManager;

    [SerializeField]
    private StateIndicator stateIndicator;

    private DashActor dashActor;
    public DashActor DashActor => dashActor;

    private WalkingActor walkingActor;
    public WalkingActor WalkingActor => walkingActor;

    private StateMachine<States> stateMachine;

    private bool isLocked = false;
    public bool IsMovementLocked
    {
        get => isLocked;
        set
        {
            if (value != isLocked)
            {
                if (!value)
                    stateMachine.SetState(stateMachine.States.DefaultMovementState);
                else
                    stateMachine.SetState(stateMachine.States.idleState);

                isLocked = value;
            }
        }
    }

    public bool IsWalking => stateMachine.IsStateCurrent(stateMachine.States.runState);
    public bool IsDashing => stateMachine.IsStateCurrent(stateMachine.States.dashState);

    private void Awake()
    {
        dashActor = GetComponent<DashActor>();
        walkingActor = GetComponent<WalkingActor>();

        stateMachine = new StateMachine<States>(States.FromComponent(this), states => {

            OnDashInputBegin += () =>
            {
                var movementDirection = walkingActor.TargetMovementDirection;
                if (CanDash(movementDirection))
                {
                    states.dashState.Direction = movementDirection.normalized;
                    stateMachine.SetState(states.dashState);
                }
            };

            dashActor.OnDashStart += () =>
            {
                batteryManager.AddBatteryValue(-1);
            };

            dashActor.OnDashEnd += () =>
            {
                if (stateMachine.IsStateCurrent<DashState>())
                {
                    stateMachine.SetState(states.DefaultMovementState);
                }
            };

            return states.DefaultMovementState;
        });

        stateIndicator?.Bind(stateMachine);
    }

    private bool CanDash(Vector2 direction)
    {
        return direction.SqrMagnitude() > 0f
            && stateMachine.IsStateCurrent(stateMachine.States.runState)
            && batteryManager.BatteryValue > 0;
    }

    private void Update()
    {
        this.stateMachine.CurrentState.Update();
    }

    private class DashState : State
    {
        private PlayerMovementController controller;

        public override string Name => "DashState";

        public Vector2 Direction;

        public DashState(PlayerMovementController controller)
        {
            this.controller = controller;
        }

        public override void OnEnter(IStateMachine sm)
        {
            controller.OnDashStateEnter?.Invoke();
            controller.dashActor.DashInDirection(Direction);
        }

        public override void OnExit(IStateMachine sm)
        {
            controller.OnDashStateExit?.Invoke();
        }
    }
}
