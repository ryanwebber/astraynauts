using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class RaycastBody : MonoBehaviour
{
    public struct Collision
    {
        public Ray2D ray;
        public RaycastHit2D hit;

        public Vector2 Translation => hit.point - ray.origin;

        public static implicit operator bool(Collision collision)
        {
            return collision.hit;
        }
    }

    [SerializeField]
    [Min(2)]
    private int numRays;

    [SerializeField]
    [Min(1)]
    private int maxCollisions = 4;

    [SerializeField]
    private LayerMask collisionMask;

    private CircleCollider2D bodyCollider;

    private int previousNumCollisions;
    private Collision[] previousCollisions;

    public int CollisionCount => previousNumCollisions;

    private void Awake()
    {
        bodyCollider = GetComponent<CircleCollider2D>();
        previousCollisions = new Collision[maxCollisions];
    }

    public Collision GetCollision(int index)
    {
        return previousCollisions[index];
    }

    public Vector2 MoveAndBounce(Vector2 delta)
    {
        float currentDelta = 0f;
        float targetDelta = delta.magnitude;
        int numCollisions = 0;

        Vector2 velocity = delta;
        Vector2 position = transform.position;

        while (numCollisions < previousCollisions.Length && currentDelta < targetDelta)
        {
            var minHitDistance = float.PositiveInfinity;
            Collision resolvedCollision = default;

            foreach (var ray in GetRays(position, velocity))
            {

                // This would be a problem if we "push" against walls, but we bounce
                if (Physics2D.OverlapPoint(ray.origin, collisionMask))
                    continue;


                Debug.DrawRay(ray.origin, ray.direction.normalized * velocity.magnitude, Color.gray);
                var _raycastHit = Physics2D.Raycast(ray.origin, ray.direction, velocity.magnitude, collisionMask);
                if (_raycastHit && _raycastHit.distance < minHitDistance)
                {
                    minHitDistance = _raycastHit.distance;
                    resolvedCollision = new Collision
                    {
                        ray = ray,
                        hit = _raycastHit
                    };
                }
            }

            if (resolvedCollision)
            {
                var translation = resolvedCollision.Translation;

                currentDelta += translation.magnitude;
                previousCollisions[numCollisions++] = resolvedCollision;

                position += translation - translation.normalized * 0.0001f;
                velocity = Vector2.Reflect(velocity, resolvedCollision.hit.normal.normalized).normalized * Mathf.Max(0, targetDelta - currentDelta);
            }
            else
            {
                position += velocity;
                break;
            }
        }

        previousNumCollisions = numCollisions;
        transform.position = position;

        return velocity.normalized;
    }

    private IEnumerable<Ray2D> GetRays(Vector2 origin, Vector2 delta)
    {
        if (delta.sqrMagnitude == 0f)
            yield break;

        if (bodyCollider == null)
            bodyCollider = GetComponent<CircleCollider2D>();

        var center = origin + bodyCollider.offset;
        var radius = bodyCollider.radius;
        var radiusSqr = radius * radius;

        var forwardDir = delta.normalized;
        var crossDir = (Vector2)Vector3.Cross(forwardDir, Vector3.back).normalized;
        var step = 1 / ((float)numRays - 1);

        for (int i = 0; i < numRays; i++)
        {
            var crossOffset = Vector2.Lerp(crossDir, -crossDir, step * i) * radius;
            var forwardMagnitude = Mathf.Sqrt(Mathf.Max(0, radiusSqr - crossOffset.magnitude * crossOffset.magnitude));

            yield return new Ray2D(center + crossOffset + delta.normalized * forwardMagnitude, forwardDir);
        }
    }
}
