using UnityEngine;
using System.Collections;

using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;

[RequireComponent(typeof(PlayerInputState))]
public class PlayerStateController : MonoBehaviour
{
    public Event OnStateChanged;

    public enum State
    {
        Frozen, Charging, FreeMoving, Dead
    }

    public struct MovementState
    {
        public bool isMoving;
    }

    [SerializeField]
    private Player player;

    [SerializeField]
    private GridLockedBody gridBody;

    [SerializeField]
    private ProjectileSpawner projectileSpawner;

    [SerializeField]
    private BatteryManager batteryManager;

    [SerializeField]
    private HealthManager healthManager;

    [SerializeField]
    private PlayerInputFeedback inputFeedback;

    [SerializeField]
    private float movementDelay = 0.075f;

    [SerializeField]
    private ChargingBehavior.Properties chargingProperties;

    [SerializeField]
    private SingleShotBehavior.Properties shootingProperties;

    private BehaviorTree movementTree;
    private BehaviorTree shootingTree;
    private PlayerInputState inputState;

    private SingleShotBehavior.Input shootingInput;
    private GridStepBehavior.Input gridStepInput;

    private State currentState;
    public State CurrentState => currentState;

    private MovementState movementState;
    public MovementState CurrentMovementState => movementState;

    private void Awake()
    {
        inputState = GetComponent<PlayerInputState>();
        shootingInput = new SingleShotBehavior.Input();
        gridStepInput = new GridStepBehavior.Input();

        OnStateChanged += () =>
        {
            movementTree.Reset();
            shootingTree.Reset();

            Debug.Log($"Player state is now {CurrentState}", this);

            switch (CurrentState)
            {
                case State.Charging:
                case State.FreeMoving:
                    healthManager.SetState(HealthManager.Damagability.VULNERABLE);
                    break;
                default:
                    healthManager.SetState(HealthManager.Damagability.TRANSPARENT);
                    break;
            }
        };

        player.OnPlayerWillSpawn += gameState =>
        {
            gridBody.InitializeInWorld(gameState.World);
        };

        var chargeBehavior = new ChargingBehavior(chargingProperties);
        var shootingBehavior = new SingleShotBehavior(shootingInput, shootingProperties);
        var gridStepBehavior = new GridStepBehavior(gridBody, gridStepInput, movementDelay);

        shootingBehavior.OnFireShot += FireProjectile;

        movementTree = new BehaviorTreeBuilder(gameObject)
            .Selector()
                .Sequence("Player frozen state")
                    .Condition(() => currentState == State.Frozen)
                // TODO 
                .End()
                .Sequence("Player moving state")
                    .Condition(() => currentState == State.FreeMoving)
                    .Selector()
                        .Sequence()
                            .Splice(gridStepBehavior)
                        .End()
                    .End()
                .End()
                .Sequence("Player charging state")
                    .Condition(() => currentState == State.Charging)
                    .ShortCircuit("Charging", () => currentState == State.Charging)
                        .Splice(chargeBehavior)
                    .End()
                .End()
                .Sequence("Player death state")
                    .Condition(() => currentState == State.Dead)
                // TODO
                .End()
            .End()
            .Build();

        shootingTree = new BehaviorTreeBuilder(gameObject)
            .Selector()
                .Sequence()
                    .Condition(() => currentState == State.FreeMoving)
                    .Condition(() => batteryManager.BatteryValue > 0)
                    .Splice(shootingBehavior)
                .End()
            .End()
            .Build();

        SetState(State.FreeMoving);
    }

    private void Update()
    {
        // Update input
        shootingInput.Aim = inputState.AimDirection;
        shootingInput.IsFiring = inputState.IsFiring;
        gridStepInput.Direction = inputState.MovementDirection;

        // Update movement
        movementTree.Tick();

        // Update shooting
        shootingTree.Tick();
    }

     private void FireProjectile(Vector2 direction)
     {
        inputFeedback.TriggerHapticInstant();
        projectileSpawner.SpawnProjectile(direction);
        batteryManager.AddBatteryValue(-1);
    }

    public void SetState(State newState)
    {
        currentState = newState;
        OnStateChanged?.Invoke();
    }
}
