using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class BoidInfluencer : MonoBehaviour
{
    [SerializeField]
    private Boid boid;

    public IEnumerable<Vector2> GetInfluences()
    {
        var forces = boid.ComputeForces();
        yield return forces.alignmentForce;
        yield return forces.cohesionForce;
        yield return forces.separationForce;
    }
}
