using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RopeSystem : MonoBehaviour
{
    private struct PointProperties
    {
        public bool isLocked;
    }
    
    [SerializeField, Min(1)]
    private int numIterations = 8;
    
    [SerializeField]
    private LayerMask collisionMask;

    private float[] segmentLengths;
    private PointProperties[] pointProperties;
    private Vector2[] pointPositions;
    public int PointCount => pointPositions?.Length ?? 0;

    private void Awake()
    {
        segmentLengths ??= new float[] { };
        pointProperties ??= new PointProperties[] { };
        pointPositions ??= new Vector2[] { };
    }

    public void SetPoints(IReadOnlyList<Vector2> points, Nullable<float> targetDistance = null)
    {
        pointPositions = new Vector2[points.Count];
        pointProperties = new PointProperties[points.Count];
        segmentLengths = new float[points.Count - 1];
        for (var i = 0; i < points.Count; i++)
            pointPositions[i] = points[i];
        
        for (int i = 0; i < points.Count - 1; i++)
        {
            if (targetDistance is { } distance)
                segmentLengths[i] = distance;
            else
                segmentLengths[i] = Vector2.Distance(points[i], points[i + 1]);
        }
    }

    public Vector2 GetPoint(int i) => pointPositions[i];
    public void SetPoint(int i, Vector2 position) => pointPositions[i] = position;

    public bool IsPointLocked(int i) => pointProperties[i].isLocked;
    public void SetPointLocked(int i, bool isLocked) => pointProperties[i].isLocked = isLocked;

    public void Solve()
    {
        for (var itr = 0; itr < numIterations; itr++)
        {
            for (var p = 0; p < pointPositions.Length - 1; p++)
            {
                var p1 = pointPositions[p];
                var p2 = pointPositions[p + 1];
                var targetLength = segmentLengths[p];
                
                var segmentCentre = (p1 + p2) / 2;
                var segmentDir = (targetLength == 0f ? Random.insideUnitCircle : p1 - p2).normalized;
                
                if (!pointProperties[p].isLocked)
                    pointPositions[p] = MoveAndSlide(p1, segmentCentre + segmentDir * targetLength / 2);
                
                if (!pointProperties[p + 1].isLocked)
                    pointPositions[p + 1] = MoveAndSlide(p2, segmentCentre - segmentDir * targetLength / 2);
            }
        }
    }
    
    private Vector2 MoveAndSlide(Vector2 origin, Vector2 target)
    {
        if (collisionMask == 0)
            return target;

        var hit = Physics2D.Linecast(origin, target, collisionMask);
        if (!hit)
            return target;

        var hitPoint = hit.point;
        var distanceRemaining = (1 - hit.fraction) * Vector2.Distance(origin, target);
        var perpendicularNormal = Vector2.Perpendicular(hit.normal);
        var slideDir = perpendicularNormal * Mathf.Sign(Vector2.Dot(perpendicularNormal, target - origin));
        return hitPoint + hit.normal * 0.01f + slideDir.normalized * distanceRemaining;
    }

    private void OnDrawGizmos()
    {
        if (PointCount > 0)
        {
            for (int i = 0; i < pointPositions.Length; i++)
            {
                Gizmos.color = pointProperties[i].isLocked ? Color.red : Color.yellow;
                Gizmos.DrawSphere(transform.position + (Vector3) pointPositions[i], 0.15f);
            }
        }
    }
}
