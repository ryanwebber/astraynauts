using UnityEngine;
using System.Collections;

public class PlayerMovementController : MonoBehaviour
{
    LocomotableActor actor;
    StateMachine stateMachine;

    private void Awake()
    {
        this.stateMachine = StateMachine.In(scope =>
        {
            scope.Enter(new StateBuilder("Moving")
                .Bind(actor)
                .Build()
            );
        });
    }
}
