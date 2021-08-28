using System;
using UnityEngine;

public static class PhysicsUtils
{
    public static float LerpGravity(float t, float height)
    {
        var quadratic = t - 0.5f;
        var yOffset = height - (4 * height * quadratic * quadratic);
        return yOffset;
    }

    public static float InverseLerpGravity(float a, float b, float value, float height)
    {
        return LerpGravity(Mathf.InverseLerp(a, b, value), height);
    }
}
