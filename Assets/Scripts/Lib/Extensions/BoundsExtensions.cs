﻿using System;
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
    }
}
