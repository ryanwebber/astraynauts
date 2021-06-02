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

    public Parameters Params => parameters;

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

        var perception = attachedServer.GetPerception(this);

        if (perception.flockCount > 0)
        {
            var centerOfFlock = perception.FlockCenter;
            var deltaFlockCenter = centerOfFlock - CurrentPosition;

            if (showDebug)
                Debug.DrawLine(transform.position, centerOfFlock, Color.blue);

            var alignmentForce = SteerTowards(perception.cumulativeFlockHeading) * parameters.alignmentWeight;
            var cohesionForce = SteerTowards(deltaFlockCenter) * parameters.cohesionWeight;
            var seperationForce = SteerTowards(perception.cumulativeLocalRepultion) * parameters.separationWeight;

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
        attachedServer = server;
        attachedServer.Register(this);
    }

    public void DetatchFromServer()
    {
        attachedServer?.Unregister(this);
    }
}
