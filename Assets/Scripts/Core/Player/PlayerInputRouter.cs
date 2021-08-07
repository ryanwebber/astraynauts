using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputBinder))]
public class PlayerInputRouter : MonoBehaviour
{
    [SerializeField]
    private WalkingInput walkingInput;

    [SerializeField]
    private PlayerMovementController movementController;

    [SerializeField]
    private PlayerShootingController shootingController;

    [SerializeField]
    private PlayerInteractionController interactionController;

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
        source.OnMovementSpecialAction += () => movementController.OnDashTriggered?.Invoke();
        source.OnFireBegin += () => shootingController.OnFireInputBegin?.Invoke();
        source.OnFireEnd += () => shootingController.OnFireInputEnd?.Invoke();
        source.OnInteractionBegin += () => interactionController.OnInteractionInputBegin?.Invoke();
        source.OnInteractionEnd += () => interactionController.OnInteractionInputEnd?.Invoke();
    }

    private void DetachFromInput(IInputSource source)
    {
        // Unbind from events
        source.OnMovementSpecialAction = default;
        source.OnFireBegin = default;
        source.OnFireEnd = default;
        source.OnInteractionBegin = default;
        source.OnInteractionEnd = default;
    }

    private void Update()
    {
        if (currentSource != null)
        {
            walkingInput.MovementDirection = currentSource.MovementValue;
            shootingController.AimInputValue = currentSource.AimValue;
        }
        else
        {
            walkingInput.MovementDirection = Vector2.zero;
        }
    }
}
