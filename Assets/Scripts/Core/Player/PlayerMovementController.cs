using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LocomotableActor))]
public class PlayerMovementController : MonoBehaviour
{
    private LocomotableActor actor;
    private StateMachine stateMachine;

    private void Awake()
    {
        this.actor = GetComponent<LocomotableActor>();
        this.stateMachine = StateMachine.In(scope =>
        {
            scope.Enter(new StateBuilder("Moving")
                .Bind(actor)
                .Build()
            );
        });
    }
}
