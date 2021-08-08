using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Mob))]
public class MobLifecycleController : MonoBehaviour
{
    private struct States
    {
        public TeleportingState teleportingState;
        public MainState mainState;

        // Death state is just a marker, the mob should have a destruction
        // trigger component handling death
        public EmptyState deathState;

        public static States FromController(MobLifecycleController controller)
        {
            return new States
            {
                teleportingState = new TeleportingState(controller.healthManager),
                mainState = new MainState(controller),
                deathState = new EmptyState("DeathState")
            };
        }
    }

    [SerializeField]
    private HealthManager healthManager;

    // The bounds of the main state
    public Event OnBeginMobControl;
    public Event OnEndMobControl;

    private StateMachine<States> stateMachine;

    private void Awake()
    {
        var mob = GetComponent<Mob>();
        stateMachine = new StateMachine<States>(States.FromController(this), states =>
        {
            // When the teleportation completes, begin the main sequence
            mob.OnDidSpawnIntoWorld += () => stateMachine.SetState(states.mainState);

            // When the mob is defeated, begin the death sequence
            mob.OnMobDefeated += () => stateMachine.SetState(states.deathState);

            return states.teleportingState;
        });
    }

    public void Bind(IActivatable activatable)
    {
        OnBeginMobControl += () => activatable.IsActive = true;
        OnEndMobControl += () => activatable.IsActive = false;
    }

    public State Bind<T>(System.Func<StateMachine<T>> stateMachineGetter, State entryState)
    {
        var teleportState = new EmptyState("TeleportState");
        var deathState = new EmptyState("DeathState");
        OnBeginMobControl += () => stateMachineGetter?.Invoke().SetState(entryState);
        OnEndMobControl += () => stateMachineGetter?.Invoke().SetState(deathState);

        return teleportState;
    }

    private class MainState : State
    {
        public override string Name => "MainState";

        private MobLifecycleController controller;

        public MainState(MobLifecycleController controller)
        {
            this.controller = controller;
        }

        public override void OnEnter(IStateMachine sm)
        {
            controller.OnBeginMobControl?.Invoke();
        }

        public override void OnExit(IStateMachine sm)
        {
            controller.OnEndMobControl?.Invoke();
        }
    }
}
