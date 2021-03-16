using System;
using UnityEngine;

public struct VectorTweenable : ITweenable<Vector2>
{
    public Vector2 from;
    public Vector2 to;

    public VectorTweenable(Vector2 from, Vector2 to)
    {
        this.from = from;
        this.to = to;
    }

    public Vector2 Interpolate(float t)
    {
        return Vector2.LerpUnclamped(from, to, t);
    }
}
