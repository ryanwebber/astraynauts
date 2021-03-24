using System;
using System.Collections.Generic;
using UnityEngine;

public static class PolygonUtils
{
    public static bool ConvexOverlap(IReadOnlyList<Vector2> aPoly, IReadOnlyList<Vector2> bPoly)
    {
        foreach (var polygon in new[] { aPoly, bPoly })
        {
            for (int i1 = 0; i1 < polygon.Count; i1++)
            {
                int i2 = (i1 + 1) % polygon.Count;
                var p1 = polygon[i1];
                var p2 = polygon[i2];

                var normal = new Vector2(p2.y - p1.y, p1.x - p2.x);

                double? minA = null, maxA = null;
                foreach (var p in aPoly)
                {
                    var projected = normal.x * p.x + normal.y * p.y;
                    if (minA == null || projected < minA)
                        minA = projected;
                    if (maxA == null || projected > maxA)
                        maxA = projected;
                }

                double? minB = null, maxB = null;
                foreach (var p in bPoly)
                {
                    var projected = normal.x * p.x + normal.y * p.y;
                    if (minB == null || projected < minB)
                        minB = projected;
                    if (maxB == null || projected > maxB)
                        maxB = projected;
                }

                if (maxA < minB || maxB < minA)
                    return false;
            }
        }
        return true;
    }
}
