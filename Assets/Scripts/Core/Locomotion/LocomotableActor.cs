using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(KinematicBody))]
[RequireComponent(typeof(LocomotableInput))]
public class LocomotableActor : MonoBehaviour, IActivatable
{

    [SerializeField]
    private LocomotionProperties properties;

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

    private LocomotableInput virtualInput;
    private KinematicBody kinematicBody;

    private Vector2 NormalizedFacingDirection
    {
        get
        {
            var facingDirection = virtualInput.FacingDirection;
            if (facingDirection == Vector2.zero)
                facingDirection = Vector2.down;

            return facingDirection.normalized;
        }
    }


    private void Awake()
    {
        kinematicBody = GetComponent<KinematicBody>();
        virtualInput = GetComponent<LocomotableInput>();
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
}
