using System;
using System.Collections.Generic;
using UnityEngine;

public class Polygon: Shape
{
    public readonly Vector2[] points;

    public Polygon(IReadOnlyList<Vector2> points)
    {
        this.points = new Vector2[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            this.points[i] = points[i];
        }
    }

    public Polygon(Vector2[] points)
    {
        this.points = points;
    }

    public override IEnumerable<Vector2> GetPoints()
    {
        return points;
    }
}
