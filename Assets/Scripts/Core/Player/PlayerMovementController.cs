using UnityEngine;
using System.Collections;
using System.Threading;

[RequireComponent(typeof(LocomotableActor))]
[RequireComponent(typeof(SpideringActor))]
public class PlayerMovementController : MonoBehaviour
{
    [System.Serializable]
    public enum MovementType
    {
        RUNNING, SPIDERING
    }

    private struct Context
    {
        public LocomotableActor locomotion;
        public SpideringActor spidering;
    }

    [SerializeField]
    private MovementType movementType;

    private StateMachine<Context> stateMachine;

    private void Awake()
    {
        var context = new Context
        {
            locomotion = GetComponent<LocomotableActor>(),
            spidering = GetComponent<SpideringActor>()
        };

        var initialState = movementType == MovementType.RUNNING ?
            (State<Context>)new RunState() : new SpiderState();

        stateMachine = new StateMachine<Context>(context, initialState);
        stateMachine.OnStateChanged += (from, to) => Debug.Log($"Player state changed: {from.Name} => {to.Name}", gameObject);
    }

    private void Update()
    {
        this.stateMachine.CurrentState.Update();
    }

    private class RunState: ComponentActivationState<Context>
    {
        public override string Name => "RunState";
        protected override IActivatable GetComponent(Context ctx) => ctx.locomotion;
    }

    private class SpiderState: ComponentActivationState<Context>
    {
        public override string Name => "SpiderState";
        protected override IActivatable GetComponent(Context ctx) => ctx.spidering;
    }
}

