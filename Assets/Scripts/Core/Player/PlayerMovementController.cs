using UnityEngine;
using System.Collections;
using System.Threading;

[RequireComponent(typeof(LocomotableActor))]
public class PlayerMovementController : MonoBehaviour
{
    private struct Context
    {
        public LocomotableActor locomotion;
    }

    private StateMachine<Context> stateMachine;

    private void Awake()
    {
        var context = new Context
        {
            locomotion = GetComponent<LocomotableActor>(),
        };

        stateMachine = new StateMachine<Context>(context, new MoveState());
        stateMachine.OnStateChanged += (from, to) => Debug.Log($"Player state changed: {from.Name} => {to.Name}", gameObject);
    }

    private void Update()
    {
        this.stateMachine.CurrentState.Update();
    }

    private class MoveState : State<Context>
    {
        public override string Name => "MoveState";
        public override void OnEnter(StateMachine<Context> sm) => sm.Context.locomotion.IsActive = true;
        public override void OnExit(StateMachine<Context> sm) => sm.Context.locomotion.IsActive = false;
    }
}
