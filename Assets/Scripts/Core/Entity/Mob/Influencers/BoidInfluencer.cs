using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Boid))]
public class BoidInfluencer : BaseInfluencer
{
    [SerializeField]
    private MobInitializable initializer;

    [SerializeField]
    private float alignmentWeight;

    [SerializeField]
    private float separationWeight;

    [SerializeField]
    private float cohesionWeight;

    private Boid boid;

    private void Awake()
    {
        boid = GetComponent<Boid>();
        initializer.OnMobInitialize += (_, ctx) => boid.AttachToManager(ctx.Services.MobManager.BoidManager);
    }

    public override IEnumerable<Vector2> GetInfluences()
    {
        var forces = boid.ComputeForces(alignmentWeight, separationWeight, cohesionWeight);
        yield return forces.alignmentForce;
        yield return forces.cohesionForce;
        yield return forces.separationForce;
    }
}
