using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Extensions;

public abstract class Shape
{
    public abstract IEnumerable<Vector2> GetPoints();

    public Bounds BoundingRect
    {
        get
        {
            float minX = 0f;
            float maxX = 0f;
            float minY = 0f;
            float maxY = 0f;
            foreach (var point in GetPoints())
            {
                minX = Mathf.Min(minX, point.x);
                maxX = Mathf.Max(maxX, point.x);
                minY = Mathf.Min(minY, point.y);
                maxY = Mathf.Max(maxY, point.y);
            }

            Vector2 min = new Vector2(minX, minY);
            Vector2 max = new Vector2(maxX, maxY);
            Vector2 center = (max + min) / 2f;
            Vector2 size = max - min;

            return new Bounds(center, size);
        }
    }

    public Shape BoundingShape
    {
        get
        {
            var boundingShape = BoundingRect;
            var points = new Vector2[]
            {
                boundingShape.GetTopLeft(),
                boundingShape.GetTopRight(),
                boundingShape.GetBottomRight(),
                boundingShape.GetBottomLeft(),
            };

            return new Polygon(points);
        }
    }

    public IEnumerable<Edge> GetEdges()
    {
        foreach (var (p1, p2) in Collections.Pair(GetPoints(), true))
        {
            yield return new Edge(p1, p2);
        }
    }
}
