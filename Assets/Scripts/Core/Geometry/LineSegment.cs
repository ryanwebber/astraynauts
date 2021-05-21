using System;
using UnityEngine;

public struct LineSegment
{
    public Vector2 p1;
    public Vector2 p2;

    public LineSegment(Vector2 p1, Vector2 p2)
    {
        this.p1 = p1;
        this.p2 = p2;
    }

    public override string ToString()
    {
        return $"LineSegment p1={p1} p2={p2}";
    }
}
