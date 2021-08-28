using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WalkingInput))]
public class WalkingActor : MonoBehaviour, IActivatable
{

    [SerializeField]
    private KinematicBody kinematicBody;

    [SerializeField]
    private WalkingProperties properties;

    [SerializeField]
    [ReadOnly]
    private Vector2 velocity;

    [SerializeField]
    [ReadOnly]
    private Vector2 previousHeading;

    [SerializeField]
    private bool isActive;
    public bool IsActive
    {
        get => isActive;
        set => isActive = value;
    }

    public Vector2 TargetMovementDirection => virtualInput.MovementDirection;
    public Vector2 CurrentVelocity => velocity;

    private WalkingInput virtualInput;

    private void Awake()
    {
        virtualInput = GetComponent<WalkingInput>();
    }

    private void Update()
    {
        if (!isActive)
            return;

        var currentHeading = previousHeading;
        var targetHeading = Vector2.ClampMagnitude(virtualInput.MovementDirection, 1f);

        var dampenedHeading = Vector2.SmoothDamp(
            current: currentHeading,
            target: targetHeading,
            currentVelocity: ref velocity,
            smoothTime: properties.movementDampening);
            
        kinematicBody.MoveAndCollide(dampenedHeading * Time.deltaTime * properties.movementSpeed);

        previousHeading = dampenedHeading;
    }

    public void EraseMomentum()
    {
        velocity = Vector2.zero;
    }
}
