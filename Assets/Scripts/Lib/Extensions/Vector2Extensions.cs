using UnityEngine;

namespace Extensions
{
    public static class Vector2Extensions
    {
        public static Vector2 SignedHorizontal(this Vector2 v) =>
            new Vector2(System.Math.Sign(v.x), 0);

        public static Vector2 SignedVertical(this Vector2 v) =>
            new Vector2(0, System.Math.Sign(v.y));
    }
}
