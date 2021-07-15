using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ProjectileSpawner))]
[RequireComponent(typeof(PlayerInputFeedback))]
public class PlayerShootingController : MonoBehaviour
{
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

    private Input input;
    private StateMachine<States> stateMachine;

    private void Awake()
    {
        var inputFeedback = GetComponent<PlayerInputFeedback>();
        var projectileSpawner = GetComponent<ProjectileSpawner>();

        this.input = new Input();
        stateMachine = new StateMachine<States>(new States
            {
                idle = new IdleState(input),
                charging = new ChargingState(inputFeedback, input),
                firing = new FiringState(projectileSpawner, input)
            },
            states =>
            {
                states.idle.OnChargeStart += () => stateMachine.SetState(states.charging);
                states.charging.OnChargeReleased += burstCount =>
                {
                    if (burstCount <= 0)
                        stateMachine.SetState(states.idle);
                    else
                        states.firing.BurstCount = burstCount;
                        stateMachine.SetState(states.firing);
                };

                states.firing.OnFiringComplete += () => stateMachine.SetState(states.idle);

                return states.idle;
            }
        );

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

        private Input input;

        public IdleState(Input input)
        {
            this.input = input;
        }

        public override void OnUpdate(IStateMachine sm)
        {
            if (input.isFiring)
                OnChargeStart?.Invoke();
        }
    }

    private class ChargingState : State
    {
        public Event<int> OnChargeReleased;
        public override string Name => "Charging";

        private float startTime;
        private Input input;
        private PlayerInputFeedback feedback;

        public ChargingState(PlayerInputFeedback feedback, Input input)
        {
            this.input = input;
            this.feedback = feedback;
        }

        public override void OnEnter(IStateMachine sm)
        {
            startTime = Time.time;
        }

        public override void OnUpdate(IStateMachine sm)
        {
            // TODO: Make these properties
            var chargeNotchTime = 0.5f;
            var maxBurstCount = 3;

            var currentHeldDuration = Time.time - startTime;
            int currentFrameBurstAmount = Mathf.Min(Mathf.FloorToInt(currentHeldDuration / chargeNotchTime), maxBurstCount);

            if (!input.isFiring)
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
                    feedback.TriggerHapticInstant();
                }
                else if (currentFrameBurstAmount >= maxBurstCount)
                {

                    IEnumerable<float> GenerateHaptics()
                    {
                        while (input.isFiring)
                            yield return 0.5f;
                    }

                    feedback.TriggerHapticSession(GenerateHaptics());
                }
            }
        }
    }

    private class FiringState : CoroutineState
    {
        public Event OnFiringComplete;
        public override string Name => "Firing";

        public int BurstCount = 0;

        private Input input;
        private ProjectileSpawner spawner;

        public FiringState(ProjectileSpawner spawner, Input input): base(spawner)
        {
            this.input = input;
            this.spawner = spawner;
        }

        protected override IEnumerator GetCoroutine()
        {
            for (int i = 0; i < BurstCount; i++)
            {
                Fire(input.aimValue);
                yield return new WaitForSeconds(0.25f);
            }

            OnFiringComplete?.Invoke();
            yield break;
        }

        private void Fire(Vector2 direction)
            => spawner.SpawnProjectile(direction);
    }
}
