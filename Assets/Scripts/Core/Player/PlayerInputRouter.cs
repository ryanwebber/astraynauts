using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LocomotableInput))]
[RequireComponent(typeof(SpideringInput))]
[RequireComponent(typeof(PlayerInputBinder))]
public class PlayerInputRouter : MonoBehaviour
{
    private SpideringInput spideringInput;
    private LocomotableInput locomotionInput;
    private PlayerInputBinder inputBinder;
    private IInputSource currentSource;

    private void Awake()
    {
        spideringInput = GetComponent<SpideringInput>();
        locomotionInput = GetComponent<LocomotableInput>();
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
    }

    private void DetachFromInput(IInputSource source)
    {
        // Unbind from events
        source.OnMovementSpecialAction = default;
    }

    private void Update()
    {
        if (currentSource != null)
        {
            locomotionInput.MovementDirection = currentSource.MovementValue;
            spideringInput.MovementDirection = currentSource.MovementValue;
        }
        else
        {
            locomotionInput.MovementDirection = Vector2.zero;
        }
    }
}
