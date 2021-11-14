using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerInputBinder))]
[RequireComponent(typeof(PlayerInputState))]
public class PlayerInputRouter : MonoBehaviour
{
    [SerializeField]
    private PlayerInteractionController interactionController;

    private PlayerInputBinder inputBinder;
    private PlayerInputState inputState;
    private IInputSource currentSource;

    private void Awake()
    {
        inputBinder = GetComponent<PlayerInputBinder>();
        inputState = GetComponent<PlayerInputState>();
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
        source.OnInteractionBegin += () => interactionController.OnInteractionInputBegin?.Invoke();
        source.OnInteractionEnd += () => interactionController.OnInteractionInputEnd?.Invoke();
    }

    private void DetachFromInput(IInputSource source)
    {
        // Unbind from events
        source.OnInteractionBegin = default;
        source.OnInteractionEnd = default;
    }

    private void Update()
    {
        if (currentSource != null)
        {
            inputState.MovementDirection = currentSource.MovementValue;
            inputState.AimDirection = currentSource.AimValue;
            inputState.IsDashing = currentSource.IsDashPressed;
            inputState.IsFiring = currentSource.IsFirePressed;
        }
        else
        {
            inputState.MovementDirection = default;
            inputState.AimDirection = default;
            inputState.IsDashing = false;
            inputState.IsFiring = false;
        }
    }
}
