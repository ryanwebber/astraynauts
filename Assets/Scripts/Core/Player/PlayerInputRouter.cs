using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LocomotableInput))]
[RequireComponent(typeof(PlayerInputBinder))]
public class PlayerInputRouter : MonoBehaviour
{
    private LocomotableInput locomotionInput;
    private PlayerInputBinder inputBinder;
    private IInputSource currentSource;

    private void Awake()
    {
        locomotionInput = GetComponent<LocomotableInput>();
        inputBinder = GetComponent<PlayerInputBinder>();
    }

    private void OnEnable()
    {
        inputBinder.OnAttachToInput += (newSource) =>
        {
            DetachFromInput(currentSource);
            AttachToInput(newSource);
            currentSource = newSource;
        };

        AttachToInput(InputSourceUtils.DetachedSource);
    }

    private void AttachToInput(IInputSource source)
    {
        // TODO: Bind to events
    }

    private void DetachFromInput(IInputSource source)
    {
        // TODO: Unbind from events
    }

    private void Update()
    {
        locomotionInput.MovementDirection = currentSource.MovementValue;
    }
}
