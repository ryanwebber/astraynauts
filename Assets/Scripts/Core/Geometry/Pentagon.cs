using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Pentagon
{
    public static float CentralAngle = Mathf.PI * 2 / 5;

    public float sideLength;
    public float rotation;

    private float theta_0 => Mathf.PI / 2;

    public Vector2 Top => GetVertexAtIndex(0);
    public Vector2 TopRight => GetVertexAtIndex(1);
    public Vector2 BottomRight => GetVertexAtIndex(2);
    public Vector2 BottomLeft => GetVertexAtIndex(3);
    public Vector2 TopLeft => GetVertexAtIndex(4);

    public float SideLength => sideLength;
    public float Inradius => 0.1f * Mathf.Sqrt(25 + 10 * Mathf.Sqrt(5)) * SideLength;
    public float Circumradius => 0.1f * Mathf.Sqrt(50 + 10 * Mathf.Sqrt(5)) * SideLength;
    public float Height => Inradius + Circumradius;

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

    private Vector2 GetVertexAtIndex(int index)
    {
        float theta = rotation + theta_0 - index * CentralAngle;
        float x = Mathf.Cos(theta);
        float y = Mathf.Sin(theta);
        return new Vector2(x, y) * Circumradius;
    }

    public IEnumerable<Vector2> GetPoints()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return GetVertexAtIndex(i);
        }
    }

    public IEnumerable<Edge> GetEdges()
    {
        foreach (var (p1, p2) in Collections.Pair(GetPoints(), true))
        {
            yield return new Edge(p1, p2);
        }
    }

    public static Pentagon WithCircumradius(float circumradius, float rotation, Vector2 center)
    {
        Pentagon p;
        p.sideLength = circumradius * 2 * Mathf.Sin(Mathf.PI / 5);
        p.rotation = rotation;

        return p;
    }
}
