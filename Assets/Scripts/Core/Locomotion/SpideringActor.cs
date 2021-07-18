using Extensions;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(KinematicBody))]
[RequireComponent(typeof(SpideringInput))]
public class SpideringActor : MonoBehaviour, IActivatable
{
    private struct States
    {
        public FloatState floatState;
        public TraversingState traversingState;

        public static States FromComponent(SpideringActor actor)
        {
            return new States
            {
                floatState = new FloatState(actor.virtualInput, actor.kinematicBody, actor.floatStateProperties),
                traversingState = new TraversingState(actor.virtualInput, actor.kinematicBody, actor.traversingStateProperties),
            };
        }
    }

    [SerializeField]
    private FloatState.Properties floatStateProperties;

    [SerializeField]
    private TraversingState.Properties traversingStateProperties;

    [SerializeField]
    private bool isActive;
    public bool IsActive
    {
        get => isActive;
        set => isActive = value;
    }

    private StateMachine<States> stateMachine;
    private KinematicBody kinematicBody;
    private SpideringInput virtualInput;

    private void Awake()
    {
        this.kinematicBody = GetComponent<KinematicBody>();
        this.virtualInput = GetComponent<SpideringInput>();

        stateMachine = new StateMachine<States>(States.FromComponent(this), states => {

            states.floatState.OnFloatStateCollision += () => stateMachine.SetState(states.traversingState);
            states.traversingState.OnWallDetach += (dir) =>
            {
                states.floatState.JumpHeading = dir;
                stateMachine.SetState(states.floatState);
            };

            return states.floatState;
        });

    }

    private void Update()
    {
        if (IsActive)
            stateMachine.CurrentState?.Update();
    }
}

