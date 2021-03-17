using System;
using UnityEngine;

[System.Serializable]
public struct LocomotionProperties
{
    public float forwardSpeed;
    public float reverseSpeed;
    public float strafeSpeed;

    public float EvaluateSpeed(Vector2 direction)
    {
        // TODO: Evaluate using appropriate speed
        return forwardSpeed;
    }
}
