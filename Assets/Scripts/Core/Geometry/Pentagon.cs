using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pentagon: Shape
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

    private Pentagon(float sideLength, float rotation)
    {
        this.sideLength = sideLength;
        this.rotation = rotation;
    }

    private Vector2 GetVertexAtIndex(int index)
    {
        float theta = rotation + theta_0 - index * CentralAngle;
        float x = Mathf.Cos(theta);
        float y = Mathf.Sin(theta);
        return new Vector2(x, y) * Circumradius;
    }

    override public IEnumerable<Vector2> GetPoints()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return GetVertexAtIndex(i);
        }
    }

    public static Pentagon WithCircumradius(float circumradius, float rotation)
    {
        return new Pentagon(circumradius * 2 * Mathf.Sin(Mathf.PI / 5), rotation);
    }
}
