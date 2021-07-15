using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ProjectileSpawner))]
[RequireComponent(typeof(PlayerInputFeedback))]
public class PlayerShootingController : MonoBehaviour
{
    [System.Serializable]
    public struct Properties
    {
        [SerializeField]
        public float burstFireDelay;

        [SerializeField]
        public float burstNotchChargeTime;

        [SerializeField]
        public int maxBurstCount;
    }

    private struct States
    {
        public IdleState idle;
        public ChargingState charging;
        public FiringState firing;
    }

    private class Input
    {
        public Vector2 aimValue;
        public bool isFiring;

        public Input()
        {
            aimValue = Vector2.zero;
            isFiring = false;
        }
    }

    public Vector2 AimValue
    {
        get => input.aimValue;
        set => input.aimValue = value;
    }

    public bool IsFiring
    {
        get => input.isFiring;
        set => input.isFiring = value;
    }

    [SerializeField]
    private Properties properties;

    private Input input;
    private ProjectileSpawner projectileSpawner;
    private PlayerInputFeedback inputFeedback;

    private StateMachine<States> stateMachine;

    private void Awake()
    {
        inputFeedback = GetComponent<PlayerInputFeedback>();
        projectileSpawner = GetComponent<ProjectileSpawner>();
        input = new Input();

        stateMachine = new StateMachine<States>(new States
            {
                idle = new IdleState(this),
                charging = new ChargingState(this),
                firing = new FiringState(this)
            },
            states =>
            {
                states.idle.OnChargeStart += () => stateMachine.SetState(states.charging);
                states.charging.OnChargeReleased += burstCount =>
                {
                    if (burstCount <= 0)
                    {
                        stateMachine.SetState(states.idle);
                    }
                    else
                    {
                        states.firing.BurstCount = burstCount;
                        stateMachine.SetState(states.firing);
                    }
                };

                states.firing.OnFiringComplete += () => stateMachine.SetState(states.idle);

                return states.idle;
            }
        );

        stateMachine.OnStateChanged += (fromState, toState) =>
        {
            Debug.Log($"Shooting state changed: {fromState?.Name} -> {toState?.Name}");
        };

        projectileSpawner.Decorators += projectile =>
        {
            // TODO: Player-based decoration
        };
    }

    private void Update()
    {
        stateMachine.CurrentState.Update();
    }

    private void OnDrawGizmos()
    {
        if (input != null && input.aimValue.sqrMagnitude > 0.001f)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, input.aimValue);
        }
    }

    private class IdleState : State
    {
        public Event OnChargeStart;
        public override string Name => "Idle";

        private PlayerShootingController controller;

        public IdleState(PlayerShootingController controller)
        {
            this.controller = controller;
        }

        public override void OnUpdate(IStateMachine sm)
        {
            if (controller.input.isFiring)
                OnChargeStart?.Invoke();
        }
    }

    private class ChargingState : State
    {
        public Event<int> OnChargeReleased;
        public override string Name => "Charging";

        private float startTime;
        private PlayerShootingController controller;

        public ChargingState(PlayerShootingController controller)
        {
            this.controller = controller;
        }

        public override void OnEnter(IStateMachine sm)
        {
            startTime = Time.time;
        }

        public override void OnUpdate(IStateMachine sm)
        {
            var chargeNotchTime = controller.properties.burstNotchChargeTime;
            var maxBurstCount = controller.properties.maxBurstCount;

            var currentHeldDuration = Time.time - startTime;
            int currentFrameBurstAmount = Mathf.Min(Mathf.FloorToInt(currentHeldDuration / chargeNotchTime), maxBurstCount);

            if (!controller.input.isFiring)
            {
                OnChargeReleased?.Invoke(currentFrameBurstAmount);
            }
            else
            {
                int previousFrameBurstAmount = Mathf.FloorToInt((Time.time - Time.deltaTime - startTime) / chargeNotchTime);

                if (previousFrameBurstAmount < 0 || currentFrameBurstAmount <= 0)
                    return;

                if (previousFrameBurstAmount < currentFrameBurstAmount && currentFrameBurstAmount < maxBurstCount)
                {
                    controller.inputFeedback.TriggerHapticInstant();
                }
                else if (currentFrameBurstAmount >= maxBurstCount)
                {
                    // TODO: Generate sin-wave haptics
                }
            }
        }
    }

    private class FiringState : CoroutineState
    {
        public Event OnFiringComplete;
        public override string Name => "Firing";

        public int BurstCount = 0;

        private PlayerShootingController controller;

        public FiringState(PlayerShootingController controller): base(controller)
        {
            this.controller = controller;
        }

        protected override IEnumerator GetCoroutine()
        {
            for (int i = 0; i < BurstCount; i++)
            {
                Fire(controller.input.aimValue);
                yield return new WaitForSeconds(controller.properties.burstFireDelay);
            }

            OnFiringComplete?.Invoke();
            yield break;
        }

        private void Fire(Vector2 direction)
            => controller.projectileSpawner.SpawnProjectile(direction);
    }
}
