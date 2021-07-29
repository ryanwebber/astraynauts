using System;
using UnityEngine;

public struct Circle
{
    public Vector2 center;
    public float radius;

    public Circle(Vector2 center, float radius)
    {
        this.center = center;
        this.radius = radius;
    }

    public bool ContainsPoint(Vector2 point)
        => Vector2.Distance(center, point) <= radius;

    public static bool FromPointsOnCircumference(Vector2 p0, Vector2 p1, Vector2 p2, out Circle circle)
    {
        if (p1.x == p0.x || p2.x == p1.x)
        {
            circle = default;
            return false;
        }

        float mr = (p1.y - p0.y) / (p1.x - p0.x);
        float mt = (p2.y - p1.y) / (p2.x - p1.x);

        if (mr == mt)
        {
            circle = default;
            return false;
        }

        float x = (mr * mt * (p2.y - p0.y) + mr * (p1.x + p2.x) - mt * (p0.x + p1.x)) / (2f * (mr - mt));
        float y = (p0.y + p1.y) / 2f - (x - (p0.x + p1.x) / 2f) / mr;

        float radius = Mathf.Sqrt(Mathf.Pow(p1.x - x, 2f) + Mathf.Pow(p1.y - y, 2f));
        Vector2 center = new Vector2(x, y);

        circle = new Circle
        {
            center = center,
            radius = radius
        };

        return true;
    }
}
