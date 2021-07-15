using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputBinder))]
public class PlayerInputRouter : MonoBehaviour
{
    [SerializeField]
    private SpideringInput spideringInput;

    [SerializeField]
    private LocomotableInput locomotionInput;

    [SerializeField]
    private PlayerShootingController shootingController;

    private PlayerInputBinder inputBinder;
    private IInputSource currentSource;

    private void Awake()
    {
        inputBinder = GetComponent<PlayerInputBinder>();
    }

    private void OnEnable()
    {
        inputBinder.OnAttachToInput += (newSource) =>
        {
            if (currentSource != null)
                DetachFromInput(currentSource);
            if (newSource != null)
                AttachToInput(newSource);

            currentSource = newSource;
        };

        AttachToInput(InputSourceUtils.DetachedSource);
    }

    private void AttachToInput(IInputSource source)
    {
        // Bind to events
        source.OnMovementSpecialAction += () => spideringInput.IsJumping = true;
        source.OnFireBegin += () => shootingController.IsFiring = true;
        source.OnFireEnd += () => shootingController.IsFiring = false;
    }

    private void DetachFromInput(IInputSource source)
    {
        // Unbind from events
        source.OnMovementSpecialAction = default;
        source.OnFireBegin = default;
        source.OnFireEnd = default;
    }

    private void Update()
    {
        if (currentSource != null)
        {
            locomotionInput.MovementDirection = currentSource.MovementValue;
            spideringInput.MovementDirection = currentSource.MovementValue;
            shootingController.AimValue = currentSource.AimValue;
        }
        else
        {
            locomotionInput.MovementDirection = Vector2.zero;
            spideringInput.MovementDirection = Vector2.zero;
        }
    }
}
