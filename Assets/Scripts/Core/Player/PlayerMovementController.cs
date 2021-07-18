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

        public static States FromComponent(PlayerMovementController controller)
        {
            return new States
            {
                spiderState = new ComponentActivationState(controller.spideringActor, "SpideringState"),
                runState = new ComponentActivationState(controller.locomotableActor, "RunningState"),
            };
        }
    }

    [SerializeField]
    private bool isWeightless = false;

    private SpideringActor spideringActor;
    private LocomotableActor locomotableActor;
    private StateMachine<States> stateMachine;

    private void Awake()
    {
        this.spideringActor = GetComponent<SpideringActor>();
        this.locomotableActor = GetComponent<LocomotableActor>();

        stateMachine = new StateMachine<States>(States.FromComponent(this), states => {
            return this.isWeightless ? (State)states.spiderState : states.runState;
        });
    }

    private void Update()
    {
        this.stateMachine.CurrentState.Update();
    }
}
