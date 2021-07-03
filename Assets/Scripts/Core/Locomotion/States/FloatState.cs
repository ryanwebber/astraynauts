using System;
using UnityEngine;

public class FloatState: State, IPropertiesMutable<FloatState.Properties>
{
    [System.Serializable]
    public struct Properties
    {
        public float jumpForce;
        public float steerForce;
        public float maxVelocity;
    }

    public override string Name => "FloatState";

    private SpideringInput input;
    private KinematicBody body;
    private Properties properties;

    private Vector2 velocity;
    public Vector2 JumpHeading
    {
        set => velocity = value.normalized * properties.jumpForce;
    }

    public Event OnFloatStateCollision;

    public FloatState(SpideringInput input, KinematicBody body, Properties properties)
    {
        this.input = input;
        this.body = body;
        this.properties = properties;
        this.velocity = Vector2.zero;
    }

    public override void OnUpdate(IStateMachine sm)
    {
        velocity = Vector2.ClampMagnitude(velocity + input.MovementDirection * properties.steerForce, properties.maxVelocity);

        body.MoveAndCollide(velocity * Time.deltaTime);

        if (body.CollisionState.HasCollision)
            OnFloatStateCollision?.Invoke();
    }

    public override void OnExit(IStateMachine sm)
    {
        velocity = Vector2.zero;
    }

    public void UpdateProperties(PropertiesUpdating<Properties> updater)
        => updater?.Invoke(ref properties);
}

