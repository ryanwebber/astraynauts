using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerInteractionController))]
[RequireComponent(typeof(PlayerMovementController))]
[RequireComponent(typeof(PlayerShootingController))]
public class PlayerState : MonoBehaviour
{
    private struct States
    {
        public MainState mainState;
        public ChargingState chargingState;

        public static States FromComponent(PlayerState player)
        {
            return new States
            {
                mainState = new MainState(player),
                chargingState = new ChargingState(player)
            };
        }

        public void Reset(StateMachine<States> sm)
        {
            sm.SetState(mainState);
        }
    }

    [System.Serializable]
    public struct ChargingProperties
    {
        [SerializeField]
        [Min(0f)]
        public float chargingStepTime;
    }

    [SerializeField]
    private ChargingProperties chargingProperties;

    private PlayerMovementController movementController;
    private PlayerShootingController shootingController;
    private PlayerInteractionController interactionController;

    private StateMachine<States> stateMachine;

    private void Awake()
    {
        movementController = GetComponent<PlayerMovementController>();
        shootingController = GetComponent<PlayerShootingController>();
        interactionController = GetComponent<PlayerInteractionController>();

        stateMachine = new StateMachine<States>(States.FromComponent(this), states =>
        {
            states.chargingState.OnChargeStarted += () => Debug.Log("Player charge starting...", this);
            states.chargingState.OnChargeStep += () => Debug.Log("Player charging step ticked...", this);
            states.chargingState.OnChargeEnded += () => Debug.Log("Player charging step ended...", this);

            return states.mainState;
        });
    }

    public bool TryStartCharging(BatteryInteraction battery)
    {
        if (stateMachine.IsStateCurrent<MainState>())
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

    private class MainState : State
    {
        public override string Name => "MainState";

        private PlayerState player;

        public MainState(PlayerState player)
        {
            this.player = player;
        }

        public override void OnEnter(IStateMachine sm)
        {
            Debug.Log("Enabling all player controls");
            player.movementController.IsMovementLocked = false;
            player.shootingController.IsShootingLocked = false;
            player.interactionController.IsInteractionLocked = false;
        }
    }

    private class ChargingState : CoroutineState
    {
        public override string Name => "ChargingState";

        public Event OnChargeStarted;
        public Event OnChargeStep;
        public Event OnChargeEnded;

        private PlayerState player;
        private ChargingProperties Properties => player.chargingProperties;

        public ChargingState(PlayerState playerState): base(playerState)
        {
            this.player = playerState;
        }

        protected override IEnumerator GetCoroutine()
        {
            player.movementController.IsMovementLocked = true;
            player.shootingController.IsShootingLocked = true;
            player.interactionController.IsInteractionLocked = false;

            OnChargeStarted?.Invoke();

            // This state never exits, the state machine must be modified
            // externally
            while (true)
            {
                yield return new WaitForSeconds(Properties.chargingStepTime);
                OnChargeStep?.Invoke();
            }
        }

        protected override void OnExit()
        {
            OnChargeEnded?.Invoke();
        }
    }
}
