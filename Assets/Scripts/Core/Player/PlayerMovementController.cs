using UnityEngine;
using System.Collections;
using System.Threading;

[RequireComponent(typeof(DashActor))]
[RequireComponent(typeof(WalkingActor))]
public class PlayerMovementController : MonoBehaviour
{
    public Event OnDashTriggered;

    private struct States
    {
        public State runState;
        public DashState dashState;
        public EmptyState idleState;

        public static States FromComponent(PlayerMovementController controller)
        {
            return new States
            {
                runState = new ComponentActivationState<WalkingActor>(controller.walkingActor, "RunningState", actor =>
                {
                    actor.EraseMomentum();
                }),
                dashState = new DashState(controller),
                idleState = new EmptyState("IdleState"),
            };
        }

        public State DefaultMovementState => runState;
    }

    [SerializeField]
    private BatteryManager batteryManager;

    private DashActor dashActor;
    private WalkingActor walkingActor;

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

    private void Awake()
    {
        dashActor = GetComponent<DashActor>();
        walkingActor = GetComponent<WalkingActor>();

        stateMachine = new StateMachine<States>(States.FromComponent(this), states => {

            OnDashTriggered += () =>
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
    }

    private bool CanDash(Vector2 direction)
    {
        Debug.Log($"dir={direction} state={stateMachine.CurrentState.Name} battery={batteryManager.BatteryValue}");
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
            controller.dashActor.DashInDirection(Direction);
        }
    }
}
