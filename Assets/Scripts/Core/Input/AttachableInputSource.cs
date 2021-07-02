using UnityEngine;
using System.Collections;

using InputSystem = UnityEngine.InputSystem;

[RequireComponent(typeof(InputSystem.PlayerInput))]
public class AttachableInputSource : MonoBehaviour
{
    private RelayInputSource relayedSource;
    public IInputSource MainSource => relayedSource;

    private void Awake()
    {
        relayedSource = new RelayInputSource();
    }

    public void OnPlayerMove(InputSystem.InputAction.CallbackContext ctx)
    {
        relayedSource.MovementValue = Vector2.ClampMagnitude(ctx.ReadValue<Vector2>(), 1f);
    }

    public void OnPlayerAim(InputSystem.InputAction.CallbackContext ctx)
    {
        relayedSource.AimValue = Vector2.ClampMagnitude(ctx.ReadValue<Vector2>(), 1f);
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
