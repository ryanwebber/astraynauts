using UnityEngine;
using System.Collections;
using System.Threading;

[RequireComponent(typeof(LocomotableActor))]
[RequireComponent(typeof(SpideringActor))]
public class PlayerMovementController : MonoBehaviour
{
    private struct States
    {
        public State spiderState;
        public State runState;
        public State idleState;

        public static States FromComponent(PlayerMovementController controller)
        {
            return new States
            {
                spiderState = new ComponentActivationState<SpideringActor>(controller.spideringActor, "SpideringState", actor =>
                {
                    // TODO: remove floating velocity when re-entering the spider state
                }),
                runState = new ComponentActivationState<LocomotableActor>(controller.locomotableActor, "RunningState"),
                idleState = new EmptyState("IdleState")
            };
        }

        public State GetMovementState(bool isWeightless)
            => isWeightless ? spiderState : runState;
    }

    [SerializeField]
    private bool isWeightless = false;

    private SpideringActor spideringActor;
    private LocomotableActor locomotableActor;
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
                    stateMachine.SetState(stateMachine.States.GetMovementState(isWeightless));
                else
                    stateMachine.SetState(stateMachine.States.idleState);

                isLocked = value;
            }
        }
    }

    private void Awake()
    {
        this.spideringActor = GetComponent<SpideringActor>();
        this.locomotableActor = GetComponent<LocomotableActor>();

        stateMachine = new StateMachine<States>(States.FromComponent(this), states => {
            return states.GetMovementState(isWeightless);
        });
    }

    private void Update()
    {
        this.stateMachine.CurrentState.Update();
    }
}
