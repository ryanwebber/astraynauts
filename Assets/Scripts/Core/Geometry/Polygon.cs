using System;
using System.Collections.Generic;
using UnityEngine;

public class Polygon: MonoBehaviour
{
    [SerializeField]
    private List<Vector2> points;
    public List<Vector2> Points => points;

    private void OnValidate()
    {
        if (points == null)
        {
            points = new List<Vector2>(new Vector2[] {
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, -0.5f),
                new Vector2(-0.5f, -0.5f),
                new Vector2(-0.5f, 0.5f),
            });
        }
    }
}
