using UnityEngine;
using System.Collections;
using System.Threading;

[RequireComponent(typeof(LocomotableActor))]
[RequireComponent(typeof(SpideringActor))]
public class PlayerMovementController : MonoBehaviour
{
    private struct States
    {
        public SpiderState spiderState;

        public static States FromComponent(PlayerMovementController controller)
        {
            return new States
            {
                spiderState = new SpiderState(controller.spideringActor),
            };
        }
    }

    private SpideringActor spideringActor;
    private StateMachine<States> stateMachine;

    private void Awake()
    {
        this.spideringActor = GetComponent<SpideringActor>();
        stateMachine = new StateMachine<States>(States.FromComponent(this), states => {
            return states.spiderState;
        });
    }

    private void Update()
    {
        this.stateMachine.CurrentState.Update();
    }

    private class SpiderState: ComponentActivationState
    {
        private SpideringActor actor;

        public SpiderState(SpideringActor actor)
        {
            this.actor = actor;
        }

        public override string Name => "SpiderState";
        protected override IActivatable Component => actor;
    }
}

