using System;
using UnityEngine;

public struct Edge
{
    public Vector2 p1;
    public Vector2 p2;

    public Edge(Vector2 p1, Vector2 p2)
    {
        this.p1 = p1;
        this.p2 = p2;
    }

    public override string ToString()
    {
        return $"Edge p1={p1} p2={p2}";
    }
}
