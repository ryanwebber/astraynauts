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
    private RelayInputFeedback relayedFeedback;

    public IInputSource MainSource => relayedSource;
    public IInputFeedback MainFeedback => relayedFeedback;

    private bool IsJoystick => rawInput.currentControlScheme != InputScemeUtils.KeyboardAndMouseScheme;

    private void Awake()
    {
        rawInput = GetComponent<InputSystem.PlayerInput>();

        relayedSource = new RelayInputSource();
        relayedSource.InputIdentifier = new PlayerIdentifier(rawInput.playerIndex, rawInput.currentControlScheme);

        relayedFeedback = new RelayInputFeedback();
        relayedFeedback.OnTriggerHapticFeedback += () =>
        {
            Debug.Log("Haptics!");
            foreach (var device in rawInput.devices)
            {
                if (device is InputSystem.Gamepad gamepad)
                {
                    // TODO: rumble for a sec
                }
            }
        };
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

        public PlayerIdentifier InputIdentifier { get; set; }

        public void Reset()
        {
            MovementValue = Vector2.zero;
            AimValue = Vector2.zero;
        }
    }

    private class RelayInputFeedback : IInputFeedback
    {
        public Event OnTriggerHapticFeedback { get; set; }
    }
}
