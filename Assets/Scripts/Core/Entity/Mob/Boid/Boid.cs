using UnityEngine;
using System.Linq;

public class Boid : MonoBehaviour
{
    public struct Force
    {
        public static Force None => default;
        public Vector2 alignmentForce;
        public Vector2 cohesionForce;
        public Vector2 separationForce;
    }

    [SerializeField]
    private Heading2D heading;

    [System.Serializable]
    public struct Parameters
    {
        [Header("Weights")]

        [SerializeField]
        public float alignmentWeight;

        [SerializeField]
        public float separationWeight;

        [SerializeField]
        public float cohesionWeight;
    }

    [SerializeField]
    private bool showDebug = false;

    [SerializeField]
    private Parameters parameters;

    [Header("Body Detection")]

    [SerializeField]
    private LayerMask separationLayermask;

    [SerializeField]
    private float detectionRadius;

    private BoidManager attachedManager = null;
    private Collider2D[] reusableCollisionResults;

    public Vector2 CurrentPosition => transform.position;
    public Vector2 CurrentHeading => heading.CurrentHeading;

    public Parameters Params => parameters;

    private void Awake()
    {
        reusableCollisionResults = new Collider2D[8];
    }

    private Vector2 SteerTowards(Vector2 vector)
    {
        return vector.normalized - CurrentHeading;
    }

    private Vector2 ComputeRawSeparation()
    {
        Vector2 rawSeparationForce = Vector2.zero;
        int nCollisions = Physics2D.OverlapCircleNonAlloc(CurrentPosition, detectionRadius, reusableCollisionResults, separationLayermask);
        for (int i = 0; i < nCollisions; i++)
        {
            if (reusableCollisionResults[i].gameObject == this)
                continue;

            if (showDebug)
                Debug.DrawLine(transform.position, reusableCollisionResults[i].gameObject.transform.position, Color.red);

            Vector2 offset = reusableCollisionResults[i].gameObject.transform.position - transform.position;
            float sqrMagnitude = offset.sqrMagnitude;
            if (sqrMagnitude > 0)
                rawSeparationForce -= offset / sqrMagnitude;
        }
        
        return rawSeparationForce;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    public Force ComputeForces()
    {
        if (attachedManager == null)
            return Force.None;

        var perception = attachedManager.GetPerception(this);

        if (perception.flockCount > 0)
        {
            var centerOfFlock = perception.FlockCenter;
            var deltaFlockCenter = centerOfFlock - CurrentPosition;

            if (showDebug)
                Debug.DrawLine(transform.position, centerOfFlock, Color.blue);

            var alignmentForce = SteerTowards(perception.cumulativeFlockHeading) * parameters.alignmentWeight;
            var cohesionForce = SteerTowards(deltaFlockCenter) * parameters.cohesionWeight;
            var seperationForce = SteerTowards(ComputeRawSeparation()) * parameters.separationWeight;

            return new Force
            {
                alignmentForce = alignmentForce,
                cohesionForce = cohesionForce,
                separationForce = seperationForce
            };
        }

        return Force.None;
    }

    public void AttachToManager(BoidManager manager)
    {
        DetatchFromManager();
        attachedManager = manager;
        attachedManager.Register(this);
    }

    public void DetatchFromManager()
    {
        attachedManager?.Unregister(this);
    }
}
