using UnityEngine;
using System.Collections;
using System.Threading;

[RequireComponent(typeof(ComponentBehavior))]
public class PlayerMovementController : MonoBehaviour, IActivatable, BehaviorControlling
{
    private Event<Vector2> OnDashTriggered;

    private struct States
    {
        public State runState;
        public State idleState;
        public DashInDirectionState dashState;

        public static States FromComponent(PlayerMovementController controller)
        {
            return new States
            {
                runState = new ComponentBehaviorState(controller.walkingComponent.Behavior),
                idleState = new EmptyState("IdleState"),
                dashState = new DashInDirectionState(controller.dashActor, () => controller.stateMachine.States.DefaultMovementState),
            };
        }

        public State DefaultMovementState => runState;
    }

    [SerializeField]
    private BatteryManager batteryManager;

    [SerializeField]
    private StateIndicator stateIndicator;

    [SerializeField]
    private DashActor dashActor;

    [SerializeField]
    private WalkingComponent walkingComponent;

    private StateMachine<States> stateMachine;

    public bool IsWalking => stateMachine.IsStateCurrent(stateMachine.States.runState);
    public bool IsDashing => stateMachine.IsStateCurrent(stateMachine.States.dashState);

    public bool IsActive { get; set; }
    public ComponentBehavior Behavior { get; private set; }

    private void Awake()
    {
        stateMachine = new StateMachine<States>(States.FromComponent(this), states => {

            OnDashTriggered += direction =>
            {
                batteryManager.AddBatteryValue(-1);
                stateMachine.States.dashState.Direction = direction;
                stateMachine.SetState(stateMachine.States.dashState);
            };

            return states.DefaultMovementState;
        });

        stateIndicator?.Bind(stateMachine);

        this.Behavior = GetComponent<ComponentBehavior>()
            .Bind(this)
            .BindOnEnable((ref Event ev) =>
            {
                ev += () => stateMachine.SetState(stateMachine.States.DefaultMovementState);
            })
            .BindOnDisable((ref Event ev) =>
            {
                ev += () => stateMachine.SetState(stateMachine.States.idleState);
            });
    }

    public bool TryTriggerDash()
    {
        var movementDirection = walkingComponent.Actor.TargetMovementDirection;
        if (CanDash(movementDirection))
        {
            OnDashTriggered?.Invoke(movementDirection.normalized);
            return true;
        }

        return false;
    }

    private bool CanDash(Vector2 direction)
    {
        return direction.SqrMagnitude() > 0f
            && stateMachine.IsStateCurrent(stateMachine.States.runState)
            && batteryManager.BatteryValue > 0;
    }
}
