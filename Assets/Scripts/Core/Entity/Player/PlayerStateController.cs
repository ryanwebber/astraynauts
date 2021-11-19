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
        public bool isDashing;
    }

    [SerializeField]
    private Player player;

    [SerializeField]
    private KinematicBody kinematicBody;

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
    private Height2D heightComponent;

    [SerializeField]
    private DashBehavior.Properties dashProperties;

    [SerializeField]
    private WalkBehavior.Properties walkProperties;

    [SerializeField]
    private ChargingBehavior.Properties chargingProperties;

    [SerializeField]
    private SingleShotBehavior.Properties shootingProperties;

    private BehaviorTree movementTree;
    private BehaviorTree shootingTree;
    private PlayerInputState inputState;

    private DashBehavior.Input dashInput;
    private WalkBehavior.Input walkInput;
    private SingleShotBehavior.Input shootingInput;
    private GridStepBehavior.Input gridStepInput;

    private State currentState;
    public State CurrentState => currentState;

    private MovementState movementState;
    public MovementState CurrentMovementState => movementState;

    private void Awake()
    {
        inputState = GetComponent<PlayerInputState>();
        dashInput = new DashBehavior.Input();
        walkInput = new WalkBehavior.Input();
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

        var dashBehavior = new DashBehavior(kinematicBody, heightComponent, dashInput, dashProperties);
        var walkBehavior = new WalkBehavior(kinematicBody, walkInput, walkProperties);
        var chargeBehavior = new ChargingBehavior(chargingProperties);
        var shootingBehavior = new SingleShotBehavior(shootingInput, shootingProperties);
        var gridStepBehavior = new GridStepBehavior(gridBody, gridStepInput);

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
                        //.Sequence("Dash movement")
                        //    .Condition(() => inputState.IsDashing)
                        //    .Success(() =>
                        //    {
                        //        movementState.isDashing = true;
                        //        healthManager.SetState(HealthManager.Damagability.TRANSPARENT);
                        //    })
                        //    .Splice(dashBehavior)
                        //    .Success(() =>
                        //    {
                        //        movementState.isDashing = false;
                        //        healthManager.SetState(HealthManager.Damagability.VULNERABLE);
                        //    })
                        //.End()
                        //.Sequence("Walking movement")
                        //    .Splice(walkBehavior)
                        //.End()
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
                    .Condition(() => !movementState.isDashing)
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
        dashInput.Direction = inputState.MovementDirection;
        walkInput.Direction = inputState.MovementDirection;
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
