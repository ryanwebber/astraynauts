using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Mob))]
public class MobLifecycle : MonoBehaviour
{
    private struct States
    {
        public TeleportingState teleportingState;
        public MainState mainState;

        // Death state is just a marker, the mob should have a destruction
        // trigger component handling death
        public EmptyState deathState;

        public static States FromController(MobLifecycle controller)
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

    private class MainState : State
    {
        public override string Name => "MainState";

        private MobLifecycle controller;

        public MainState(MobLifecycle controller)
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
