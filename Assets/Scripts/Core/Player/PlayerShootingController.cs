using System;
using UnityEngine;

public class PlayerShootingController : MonoBehaviour
{
    [ReadOnly]
    public Vector2 AimDirection;

    private Nullable<float> maybeFireStartTime;

    private void Awake()
    {
        maybeFireStartTime = null;
    }

    public bool TryStartFiring()
    {
        if (maybeFireStartTime == null)
        {
            maybeFireStartTime = Time.time;
            return true;
        }

        return false;
    }

    public bool TryCommitFiring()
    {
        if (maybeFireStartTime is float fireStartTime)
        {
            maybeFireStartTime = null;
            Fire(AimDirection, Time.time - fireStartTime);
            return true;
        }

        return false;
    }

    private void Fire(Vector2 direction, float heldDuration)
    {
        Debug.Log($"Fire: {direction}@{heldDuration}s");
    }

    private void OnDrawGizmos()
    {
        if (AimDirection.sqrMagnitude > 0.001f)
        {
            Gizmos.color = maybeFireStartTime == null ? Color.red : Color.yellow;
            Gizmos.DrawRay(transform.position, AimDirection);
        }
    }
}
