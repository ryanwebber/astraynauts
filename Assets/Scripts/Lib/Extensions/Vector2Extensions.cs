using UnityEngine;

namespace Extensions
{
    public static class Vector2Extensions
    {
        public static Vector2 SignedHorizontal(this Vector2 v) =>
            new Vector2(System.Math.Sign(v.x), 0);

        public static Vector2 SignedVertical(this Vector2 v) =>
            new Vector2(0, System.Math.Sign(v.y));

        public static Vector2 Rotate(this Vector2 v, float delta)
        {
            return new Vector2(
                v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
                v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
            );
        }

        public static Vector2 SteerTowards(Vector2 position, Vector2 velocity, Vector2 target, float inverseSteerForce)
        {
            var vMag = velocity.magnitude;
            var desiredVelocity = (target - position).normalized * vMag;
            var desiredSteering = Vector2.ClampMagnitude(desiredVelocity - velocity, vMag) / inverseSteerForce;

            return (velocity + desiredSteering).normalized * vMag;
        }
    }
}
