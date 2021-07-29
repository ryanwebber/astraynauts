using Extensions;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Projectile))]
public class SeekingTrajectory : MonoBehaviour
{
    [SerializeField]
    private float velocity = 2f;

    [SerializeField]
    [Min(0)]
    private float maxSteerForce = 0.2f;

    [SerializeField]
    [Min(0)]
    private float maxSeekVision = 2f;

    [SerializeField]
    private float maxTrackTime = 0.25f;

    [SerializeField]
    private LayerMask targetMask;

    [SerializeField]
    private LayerMask obstructionMask;

    [SerializeField]
    [Min(1)]
    private int maxTargetSortCount = 4;

    [SerializeField]
    [ReadOnly]
    private GameObject currentTarget;

    private Vector2 heading = Vector2.zero;
    private bool isSpawned = false;
    private Collider2D[] reusableCastResults;

    private void Awake()
    {
        reusableCastResults = new Collider2D[maxTargetSortCount];
        GetComponent<Projectile>().OnProjectileSpawn += v =>
        {
            heading = v.normalized * velocity;
            isSpawned = true;
        };
    }

    private void Start()
    {
        StartCoroutine(Coroutines.After(2f, () => Destroy(gameObject)));
    }

    private void Update()
    {
        transform.Translate(heading * Time.deltaTime);

        if (currentTarget == null)
        {
            var collisionCount = Physics2D.OverlapCircleNonAlloc(transform.position, maxSeekVision, reusableCastResults, targetMask);
            var bestTarget = reusableCastResults.Take(collisionCount)
                .Where(c => IsTargetable(c.transform.position))
                .OrderBy(c => Vector2.Distance(transform.position, c.transform.position))
                .FirstOrDefault();

            if (bestTarget != null)
            {
                currentTarget = bestTarget.gameObject;
                StartCoroutine(TrackTarget());
            }
        }
    }

    private IEnumerator TrackTarget()
    {
        float absoluteVelocity = heading.magnitude;
        float startTime = Time.time;
        while (Time.time < startTime + maxTrackTime)
        {
            if (currentTarget == null)
                break;

            Vector2 targetPosition = currentTarget.transform.position;
            Vector2 currentPosition = transform.position;
            Vector2 targetHeading = targetPosition - currentPosition;

            // If the target is now behind us, exit out of tracking
            if (Vector2.Dot(heading, targetHeading) < 0f)
                break;

            Vector2 newHeading = Vector3.RotateTowards(heading, targetHeading, maxSteerForce, 0f);
            heading = newHeading.normalized * absoluteVelocity;

            Debug.DrawLine(currentPosition, targetPosition, Color.green);

            yield return null;
        }

        currentTarget = null;
    }

    private bool IsTargetable(Vector2 target)
    {
        Vector2 position = transform.position;
        bool IsNearby() => Vector2.Distance(position, target) <= maxSeekVision;
        bool IsSteerable() => GetSteerExclusionZones(heading).All(z => !z.ContainsPoint(target));
        bool IsForwards() => Vector2.Dot(heading, (target - position)) > 0f;
        bool IsVisible()
        {
            var ray = Physics2D.Raycast(position, target, maxSeekVision * 1.5f, obstructionMask & targetMask);
            return !ray || (ray.collider.gameObject.layer & obstructionMask.value) != obstructionMask.value;
        };

        return IsNearby() && IsForwards() && IsSteerable() && IsVisible();
    }

    private IEnumerable<Circle> GetSteerExclusionZones(Vector2 usingHeading)
    {
        foreach (var sign in new int[] { 1, -1 })
        {
            Vector2 v1 = usingHeading;
            Vector2 p1 = transform.position;

            Vector2 v2 = v1.Rotate(maxSteerForce * sign);
            Vector2 p2 = p1 + v2;

            Vector2 v3 = v2.Rotate(maxSteerForce * sign);
            Vector2 p3 = p2 + v3;

            if (Circle.FromPointsOnCircumference(p1, p2, p3, out var circle))
                yield return circle;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
        Gizmos.DrawSphere(transform.position, maxSeekVision);

        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
        foreach (var circle in GetSteerExclusionZones(isSpawned ? heading : Vector2.up * velocity))
        {
            Gizmos.DrawSphere(circle.center, circle.radius);
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(transform.position, 0.1f);
    }
}
