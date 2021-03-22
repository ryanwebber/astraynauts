using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Pentagon
{
    public static float CentralAngle = Mathf.PI * 2 / 5;

    public Vector2 center;
    public float sideLength;
    public float rotation;

    private float theta_0 => Mathf.PI / 2 + rotation;

    public Vector2 Center => center;
    public Vector2 Top => GetVertexAtIndex(0);
    public Vector2 TopRight => GetVertexAtIndex(1);
    public Vector2 BottomRight => GetVertexAtIndex(2);
    public Vector2 BottomLeft => GetVertexAtIndex(3);
    public Vector2 TopLeft => GetVertexAtIndex(4);

    public float Rotation => rotation;

    public float SideLength => sideLength;
    public float Inradius => 0.1f * Mathf.Sqrt(25 + 10 * Mathf.Sqrt(5)) * SideLength;
    public float Circumradius => 0.1f * Mathf.Sqrt(50 + 10 * Mathf.Sqrt(5)) * SideLength;
    public float Height => Inradius + Circumradius;

    private Vector2 GetVertexAtIndex(int index)
    {
        float theta = theta_0 - index * CentralAngle;
        float x = Mathf.Cos(theta);
        float y = Mathf.Sin(theta);
        return new Vector2(x, y) * Circumradius + center;
    }

    public IEnumerable<Vector2> GetPoints()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return GetVertexAtIndex(i);
        }
    }
}
