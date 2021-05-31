using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class Boid : MonoBehaviour
{
    public class Influence
    {
        public Vector2 Force = Vector2.zero;
        public float Weight = 0f;

        public Vector2 WeightedForce => Force * Weight;

        public Influence()
        {
        }

        public Influence(Vector2 force, float weight)
        {
            Force = force;
            Weight = weight;
        }
    }

    [System.Serializable]
    public struct Parameters
    {
        [SerializeField]
        public float viewRadius;

        [SerializeField]
        public float avoidanceRadius;

        [SerializeField]
        public float maxSteerForce;

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
    private BoidServer server;
    public BoidServer Server
    {
        get => server;
        set => server = value;
    }

    [SerializeField]
    private Parameters parameters;

    [SerializeField]
    private float removeMeSpeed;

    private Vector2 previousPosition;
    public Vector2 CurrentPosition => transform.position;
    public Vector2 CurrentHeading;

    private HashSet<Influence> externalInfluences;

    private void Start()
    {
        if (server == null)
            server = BoidServer.Instance;

        server.Register(this);

        externalInfluences = new HashSet<Influence>();
        previousPosition = transform.position;
    }

    private void LateUpdate()
    {
        var deltaPosition = ((Vector2)transform.position - previousPosition);
        if (deltaPosition.magnitude < 0.0001f)
            CurrentHeading = Vector2.zero;
        else
            CurrentHeading = deltaPosition.normalized;

        previousPosition = transform.position;
    }

    private void OnDestroy()
    {
        server.Unregister(this);
    }

    private Vector2 SteerTowards(Vector2 vector)
    {
        Vector2 v = vector.normalized - CurrentHeading;
        return Vector2.ClampMagnitude(v, parameters.maxSteerForce);
    }

    public Vector2 ComputeTargetHeading()
    {
        var boids = server.GetFlock(CurrentPosition);

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

                if (sqrDst < parameters.SqrAvoidanceRadius && sqrDst != 0f)
                {
                    cumAvoidanceHeading -= relPos / sqrDst;
                }
            }
        }

        var targetHeading = externalInfluences.Aggregate(Vector2.zero, (acc, influence) => acc + influence.WeightedForce);

        if (effectiveFlockSize > 0)
        {
            var centerOfFlock = cumPosition / effectiveFlockSize;
            var deltaFlockCenter = centerOfFlock - CurrentPosition;

            var alignmentForce = SteerTowards(cumHeading) * parameters.alignmentWeight;
            var cohesionForce = SteerTowards(deltaFlockCenter) * parameters.cohesionWeight;
            var seperationForce = SteerTowards(cumAvoidanceHeading) * parameters.separationWeight;

            targetHeading += alignmentForce;
            targetHeading += cohesionForce;
            targetHeading += seperationForce;
        }

        return CurrentHeading + targetHeading.normalized * parameters.maxSteerForce * Time.deltaTime;
    }

    public void AddInfluence(Influence influence)
    {
        externalInfluences.Add(influence);
    }

    public bool RemoveInfluence(Influence influence)
    {
        return externalInfluences.Remove(influence);
    }

    private void Update()
    {
        var targetDirection = ComputeTargetHeading();
        var offset = targetDirection * removeMeSpeed * Time.deltaTime;
        Debug.DrawRay(transform.position, offset * 10, Color.green);

        transform.position += (Vector3)offset;
    }
}
