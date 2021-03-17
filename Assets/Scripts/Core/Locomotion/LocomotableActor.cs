using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LocomotableInput))]
public class LocomotableActor : MonoBehaviour, IActivatable
{

    [SerializeField]
    private LocomotionProperties properties;

    [SerializeField]
    private bool isActive;
    public bool IsActive
    {
        get => isActive;
        set => isActive = value;
    }

    private LocomotableInput virtualInput;

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

    private Vector2 ClampedMovementDirection
    {
        get
        {
            var movementDirection = virtualInput.MovementDirection;
            return Vector2.ClampMagnitude(movementDirection, 1f);
        }
    }

    private void Awake()
    {
        virtualInput = GetComponent<LocomotableInput>();
    }

    private void Update()
    {
        if (!isActive)
            return;

        var movementDirection = ClampedMovementDirection;
        var facingRotation = Quaternion.FromToRotation(NormalizedFacingDirection, Vector3.up);
        var relativeMovementDirection = facingRotation * movementDirection;
        var deltaPosition = movementDirection * properties.EvaluateSpeed(relativeMovementDirection) * Time.deltaTime;
        transform.Translate(deltaPosition);
    }
}
