using UnityEngine;
using System.Collections;

public class PlayerState : MonoBehaviour
{
    private struct States
    {
        public State mainState;
        public State chargingState;

        public static States FromComponent(PlayerState player)
        {
            return new States
            {
                mainState = new ComponentBehaviorState(player.movementController.Behavior, "MainState"),
                chargingState = new ComponentBehaviorState(player.chargingController.Behavior, "ChargingState"),
            };
        }

        public void Reset(StateMachine<States> sm)
        {
            sm.SetState(mainState);
        }
    }

    [SerializeField]
    private PlayerMovementController movementController;

    [SerializeField]
    private PlayerChargingController chargingController;

    [SerializeField]
    private PlayerShootingController shootingController;

    [SerializeField]
    private PlayerInteractionController interactionController;

    private StateMachine<States> stateMachine;

    // TODO: Track this with player health and a death state
    public bool IsAlive => true;

    private void Awake()
    {
        stateMachine = new StateMachine<States>(States.FromComponent(this), states =>
        {
            return states.mainState;
        });
    }

    public bool TryStartCharging(BatteryInteraction battery)
    {
        if (stateMachine.IsStateCurrent(stateMachine.States.mainState))
        {
            void EndCharging(Player player)
            {
                Debug.Log("Player battery interaction ended. Cleaning up state...", this);
                battery.Interactable.OnPlayerInteractionEnded -= EndCharging;
                stateMachine.States.Reset(stateMachine);
            }

            battery.Interactable.OnPlayerInteractionEnded += EndCharging;

            // Transition states
            stateMachine.SetState(stateMachine.States.chargingState);

            return true;
        }
        else
        {
            Debug.Log("Player unable to start charging, ignoring interaction", this);
            return false;
        }
    }
}
