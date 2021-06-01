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
        if (!TestForCollision(heading))
            return Vector2.zero;

        if (TryFindCollisionFreeHeading(heading, out var freeHeading))
            return freeHeading * collisionAvoidanceWeight;

        return Vector2.zero;
    }

    private bool TryFindCollisionFreeHeading(Vector2 forward, out Vector2 newHeading)
    {;
        bool TryGetHeadingInternal(out Vector2 newForward)
        {
            // Avoid infinite loops
            if (stepAngle <= 0f)
            {
                newForward = default;
                return false;
            }

            float angle = stepAngle;
            while (angle < 180f)
            {
                float thetaPos = angle * Mathf.Deg2Rad;
                var testHeading = Rotate(forward, thetaPos);
                if (!TestForCollision(testHeading))
                {
                    newForward = testHeading;
                    return true;
                }

                float thetaNeg = angle * Mathf.Deg2Rad * -1;
                testHeading = Rotate(forward, thetaNeg);
                if (!TestForCollision(testHeading))
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
            Debug.DrawRay(transform.position, newHeading, Color.green);

            return true;
        }

        newHeading = default;
        return false;
    }

    private RaycastHit2D TestForCollision(Vector2 heading)
    {
        return Physics2D.Raycast(transform.position, heading, viewDistance, layerMask);
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
}
