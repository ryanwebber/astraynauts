using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Pentagon
{
    public static float CentralAngle = Mathf.PI * 2 / 5;

    public Vector2 center;
    public float radius;
    public float rotation;

    private float theta_0 => Mathf.PI / 2 + rotation;

    public Vector2 Top => GetVertexAtIndex(0);
    public Vector2 TopRight => GetVertexAtIndex(1);
    public Vector2 BottomRight => GetVertexAtIndex(2);
    public Vector2 BottomLeft => GetVertexAtIndex(3);
    public Vector2 TopLeft => GetVertexAtIndex(4);

    private Vector2 GetVertexAtIndex(int index)
    {
        float theta = theta_0 - index * CentralAngle;
        float x = Mathf.Cos(theta);
        float y = Mathf.Sin(theta);
        return new Vector2(x, y) * radius + center;
    }

    public IEnumerable<Vector2> GetPoints()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return GetVertexAtIndex(i);
        }
    }
}
