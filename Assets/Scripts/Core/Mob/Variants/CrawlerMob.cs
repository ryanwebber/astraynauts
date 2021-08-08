using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MobLifecycleController))]
[RequireComponent(typeof(SwarmMovementController))]
public class CrawlerMob : MonoBehaviour
{
    private struct States
    {
        public State swarmState;
        public State leapState;

        public static States FromController(CrawlerMob controller)
        {
            return new States
            {
                swarmState = new ComponentActivationState<SwarmMovementController>(controller.swarmController, "SwarmState"),
                leapState = new EmptyState("LeapState")
            };
        }
    }

    [SerializeField]
    private StateIndicator stateIndicator;

    private SwarmMovementController swarmController;
    private StateMachine<States> stateMachine;

    private void Awake()
    {
        swarmController = GetComponent<SwarmMovementController>();
        var lifecycle = GetComponent<MobLifecycleController>();

        stateMachine = new StateMachine<States>(States.FromController(this), states =>
        {
            return lifecycle.Bind(() => stateMachine, states.swarmState);
        });

        stateIndicator?.Bind(stateMachine);
    }
}
