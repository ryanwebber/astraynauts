using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Heading2D))]
public class CollisionAvoidanceInfluencer : MonoBehaviour
{
    [SerializeField]
    private LayerMask layerMask;

    [SerializeField]
    private float viewDistance;

    [SerializeField]
    [Range(0f, 180f)]
    private float stepAngle = 15f;

    [SerializeField]
    [Range(0f, 180f)]
    private float maxViewFieldAngle = 180f;

    [SerializeField]
    private float collisionRadius = 1f;

    [SerializeField]
    private float rayStartRadius = 0.4f;

    [SerializeField]
    private Vector2 offset;

    [SerializeField]
    private float collisionAvoidanceWeight = 1f;

    private Heading2D currentHeading;

    private void Awake()
    {
        currentHeading = GetComponent<Heading2D>();
    }

    public IEnumerable<Vector2> GetInfluences()
    {
        yield return GetCollisionAvoidanceHeading();
    }

    private Vector2 GetCollisionAvoidanceHeading()
    {
        var heading = currentHeading.CurrentHeading;
        if (!TestForCollision(heading, heading))
            return Vector2.zero;

        if (TryFindCollisionFreeHeading(heading.normalized, out var freeHeading))
            return freeHeading * collisionAvoidanceWeight;

        return Vector2.zero;
    }

    private bool TryFindCollisionFreeHeading(Vector2 forward, out Vector2 newHeading)
    {
        bool TryGetHeadingInternal(out Vector2 newForward)
        {
            // Avoid infinite loops
            if (stepAngle <= 0f)
            {
                newForward = default;
                return false;
            }

            var normalizedTargetDir = forward.normalized;

            float angle = stepAngle;
            while (angle < maxViewFieldAngle)
            {
                float thetaPos = angle * Mathf.Deg2Rad;
                var testHeading = Rotate(forward, thetaPos);
                if (!TestForCollision(testHeading, normalizedTargetDir))
                {
                    newForward = testHeading;
                    return true;
                }

                float thetaNeg = angle * Mathf.Deg2Rad * -1;
                testHeading = Rotate(forward, thetaNeg);
                if (!TestForCollision(testHeading, normalizedTargetDir))
                {
                    newForward = testHeading;
                    return true;
                }

                angle += stepAngle;
            }

            newForward = default;
            return false;
        }

        if (TryGetHeadingInternal(out newHeading))
        {
            newHeading.Normalize();
            return true;
        }

        var crossForward = Vector3.Cross(forward, Vector3.forward);
        if (TestForCollision(forward, crossForward, true))
        {
            newHeading = forward - (Vector2)crossForward;
            Debug.DrawRay((Vector2)transform.position + offset, newHeading, Color.yellow);
            return true;
        }

        var crossDown = Vector3.Cross(forward, Vector3.back);
        if (TestForCollision(forward, crossDown, true))
        {
            newHeading = forward - (Vector2)crossDown;
            Debug.DrawRay((Vector2)transform.position + offset, newHeading, Color.yellow);
            return true;
        }

        newHeading = default;
        return false;
    }

    private RaycastHit2D TestForCollision(Vector2 heading, Vector2 desiredHeading, bool special = false)
    {
        var ray = new Ray2D((Vector2)transform.position + offset + desiredHeading * rayStartRadius, heading);
        var collision = Physics2D.CircleCast(ray.origin, collisionRadius, ray.direction, viewDistance, layerMask);

        if (special)
            Debug.DrawRay(ray.origin, ray.direction * viewDistance, Color.cyan, 0.25f);
        else if (!collision)
            Debug.DrawRay(ray.origin, ray.direction * viewDistance, Color.green);
        else
            Debug.DrawRay(ray.origin, ray.direction * viewDistance, Color.red);

        return collision;
    }

    private Vector2 Rotate(Vector2 heading, float theta)
    {
        float sin = Mathf.Sin(theta);
        float cos = Mathf.Cos(theta);

        float tx = heading.x;
        float ty = heading.y;

        return new Vector2(
             (cos * tx) - (sin * ty),
             (sin * tx) + (cos * ty)
        );
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position + (Vector3)offset, collisionRadius);
    }
}
