using UnityEngine;
using System.Collections;

[RequireComponent(typeof(KinematicBody))]
[RequireComponent(typeof(SpideringInput))]
public class SpideringActor : MonoBehaviour, IActivatable
{
    private struct Context
    {
        public SpideringInput virtualInput;
        public KinematicBody kinematicBody;
    }

    [SerializeField]
    private float jumpForce = 5f;

    [SerializeField]
    private bool isActive;
    public bool IsActive
    {
        get => isActive;
        set => isActive = value;
    }

    private StateMachine<Context> stateMachine;

    private void Awake()
    {
        var context = new Context
        {
            kinematicBody = GetComponent<KinematicBody>(),
            virtualInput = GetComponent<SpideringInput>()
        };

        stateMachine = new StateMachine<Context>(context, new FloatState(jumpForce));
    }

    private void Update()
    {
        stateMachine.CurrentState?.Update();
    }

    private class FloatState : State<Context>
    {
        public Vector2 velocity = Vector2.zero;
        public override string Name => "FloatState";

        private float jumpForce;

        public FloatState(float jumpForce)
        {
            this.jumpForce = jumpForce;
        }

        public override void OnUpdate(StateMachine<Context> sm)
        {
            if (sm.Context.virtualInput.IsJumping)
                velocity = sm.Context.virtualInput.MovementDirection;

            sm.Context.kinematicBody.MoveAndCollide(velocity * Time.deltaTime * jumpForce);
        }
    }
}
