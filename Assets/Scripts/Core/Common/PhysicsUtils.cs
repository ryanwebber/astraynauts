using System;
using UnityEngine;

public static class PhysicsUtils
{
    public static Vector2 LerpGravity(Vector2 a, Vector2 b, float t, float height)
    {
        var quadratic = t - 0.5f;
        var yOffset = height - (4 * height * quadratic * quadratic);
        return Vector2.Lerp(a, b, t) + Vector2.up * yOffset;
    }

    public static Vector2 LerpGravity(float t, float height)
    {
        return LerpGravity(Vector2.zero, Vector2.zero, t, height);
    }
}
