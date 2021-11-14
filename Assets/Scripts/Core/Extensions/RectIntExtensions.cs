using System;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{
    public static class RectIntExtensions
    {
        public static IEnumerable<Vector2Int> GetAllPositions(this RectInt rect)
        {
            foreach (var pos in rect.allPositionsWithin)
                yield return pos;
        }
    }
}
