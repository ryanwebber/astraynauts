using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Extensions
{
    public static class BoundsExtensions
    {
        public static Vector2 GetTopLeft(this Bounds bounds)
        {
            return new Vector2(bounds.min.x, bounds.max.y);
        }

        public static Vector2 GetTopRight(this Bounds bounds)
        {
            return bounds.max;
        }

        public static Vector2 GetBottomLeft(this Bounds bounds)
        {
            return bounds.min;
        }

        public static Vector2 GetBottomRight(this Bounds bounds)
        {
            return new Vector2(bounds.max.x, bounds.min.y);
        }

        public static IEnumerable<Vector2> GetCorners(this Bounds bounds)
        {
            return GetMappedCorners(bounds).Select(c => c.position);
        }

        public static IEnumerable<(Vector2 position, Vector2 corner)> GetMappedCorners(this Bounds bounds)
        {
            yield return (position: bounds.GetTopRight(), corner: new Vector2(1, 1));
            yield return (position: bounds.GetBottomRight(), corner: new Vector2(1, -1));
            yield return (position: bounds.GetBottomLeft(), corner: new Vector2(-1, -1));
            yield return (position: bounds.GetTopLeft(), corner: new Vector2(-1, 1));
        }
    }
}
