using UnityEngine;
using System.Collections;

using InputSystem = UnityEngine.InputSystem;

[RequireComponent(typeof(InputSystem.PlayerInput))]
public class AttachableInputSource : MonoBehaviour
{
    [SerializeField]
    private Transform relativeAimObject;
    public Transform RelativeAimObject
    {
        get => relativeAimObject;
        set => relativeAimObject = value;
    }

    private InputSystem.PlayerInput rawInput;
    private RelayInputSource relayedSource;
    public IInputSource MainSource => relayedSource;

    private bool IsJoystick => !rawInput.currentControlScheme.Contains("Mouse");

    private void Awake()
    {
        rawInput = GetComponent<InputSystem.PlayerInput>();
        relayedSource = new RelayInputSource();
    }

    public void OnPlayerMove(InputSystem.InputAction.CallbackContext ctx)
    {
        relayedSource.MovementValue = Vector2.ClampMagnitude(ctx.ReadValue<Vector2>(), 1f);
    }

    public void OnPlayerAim(InputSystem.InputAction.CallbackContext ctx)
    {
        if (CameraProjection.TryGetCurrent(out var projection) && !IsJoystick)
        {
            if (relativeAimObject == null)
            {
                relayedSource.AimValue = Vector2.zero;
                return;
            }

            var absoluteAimScreenPosition = ctx.ReadValue<Vector2>();
            var absoluteViewportPosition = projection.ScreenToViewport(absoluteAimScreenPosition);
            var absoluteRelativeObjectCenter = projection.WorldToViewport(relativeAimObject.position);

            // Half the screen width => magnitude of 1f
            relayedSource.AimValue = Vector2.ClampMagnitude(absoluteViewportPosition - absoluteRelativeObjectCenter, 0.5f) * 2;
        }
        else
        {
            relayedSource.AimValue = Vector2.ClampMagnitude(ctx.ReadValue<Vector2>(), 1f);
        }
    }

    public void OnPlayerFire(InputSystem.InputAction.CallbackContext ctx)
    {
        if (ctx.canceled)
            relayedSource.OnFireEnd?.Invoke();

        if (ctx.started)
            relayedSource.OnFireBegin?.Invoke();
    }

    public void OnPlayerMovementSpecialAction(InputSystem.InputAction.CallbackContext ctx)
    {
        if (ctx.started)
            relayedSource.OnMovementSpecialAction?.Invoke();
    }

    private class RelayInputSource : IInputSource
    {
        public Event OnFireBegin { get; set; }
        public Event OnFireEnd { get; set; }
        public Event OnMovementSpecialAction { get; set; }

        public Vector2 MovementValue { get; set; } = Vector2.zero;
        public Vector2 AimValue { get; set; } = Vector2.zero;

        public void Reset()
        {
            MovementValue = Vector2.zero;
            AimValue = Vector2.zero;
        }
    }
}
