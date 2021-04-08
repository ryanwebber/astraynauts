using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PolygonCollider2D))]
public class CameraConstraint : MonoBehaviour
{
    private PolygonCollider2D boundsCollider;

    private void Awake()
    {
        boundsCollider = GetComponent<PolygonCollider2D>();
    }

    public void SetBounds(IEnumerable<Vector2> points)
    {
        boundsCollider.SetPath(0, new List<Vector2>(points));
    }
}
