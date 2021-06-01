using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Heading2D))]
public class Boid : MonoBehaviour
{
    public struct Force
    {
        public static Force None => default;
        public Vector2 alignmentForce;
        public Vector2 cohesionForce;
        public Vector2 separationForce;
    }

    [System.Serializable]
    public struct Parameters
    {
        [SerializeField]
        public float viewRadius;

        [SerializeField]
        public float avoidanceRadius;

        [Header("Weights")]

        [SerializeField]
        public float alignmentWeight;

        [SerializeField]
        public float separationWeight;

        [SerializeField]
        public float cohesionWeight;

        public float SqrViewRadius => viewRadius * viewRadius;
        public float SqrAvoidanceRadius => avoidanceRadius * avoidanceRadius;
    }

    [SerializeField]
    private bool showDebug = false;

    [SerializeField]
    private Parameters parameters;

    private BoidServer attachedServer = null;
    private Heading2D heading;

    public Vector2 CurrentPosition => transform.position;
    public Vector2 CurrentHeading => heading.CurrentHeading;

    private void Awake()
    {
        heading = GetComponent<Heading2D>();
    }

    private Vector2 SteerTowards(Vector2 vector)
    {
        return vector.normalized - CurrentHeading;
    }

    public Force ComputeForces()
    {
        if (attachedServer == null)
            return Force.None;

        var boids = attachedServer.GetFlock(CurrentPosition);

        var cumHeading = Vector2.zero; // Alignment
        var cumPosition = Vector2.zero; // Coherence
        var cumAvoidanceHeading = Vector2.zero; // Separation

        int effectiveFlockSize = 0;

        foreach (var otherBoid in boids.Where(b => b != this))
        {
            Vector2 relPos = otherBoid.CurrentPosition - CurrentPosition;
            float sqrDst = relPos.SqrMagnitude();

            if (sqrDst < parameters.SqrViewRadius)
            {
                effectiveFlockSize++;

                cumHeading += otherBoid.CurrentHeading;
                cumPosition += otherBoid.CurrentPosition;

                if (showDebug)
                    Debug.DrawLine(transform.position, otherBoid.CurrentPosition, Color.gray);

                if (sqrDst < parameters.SqrAvoidanceRadius && sqrDst != 0f)
                {
                    cumAvoidanceHeading -= relPos / sqrDst;
                }
            }
        }

        if (effectiveFlockSize > 0)
        {
            var centerOfFlock = cumPosition / effectiveFlockSize;
            var deltaFlockCenter = centerOfFlock - CurrentPosition;

            if (showDebug)
                Debug.DrawLine(transform.position, centerOfFlock, Color.blue);

            var alignmentForce = SteerTowards(cumHeading) * parameters.alignmentWeight;
            var cohesionForce = SteerTowards(deltaFlockCenter) * parameters.cohesionWeight;
            var seperationForce = SteerTowards(cumAvoidanceHeading) * parameters.separationWeight;

            return new Force
            {
                alignmentForce = alignmentForce,
                cohesionForce = cohesionForce,
                separationForce = seperationForce
            };
        }

        return Force.None;
    }

    public void AttachToServer(BoidServer server)
    {
        DetatchFromServer();
        this.attachedServer = server;
        this.attachedServer.Register(this);
    }

    public void DetatchFromServer()
    {
        this.attachedServer?.Unregister(this);
    }
}
