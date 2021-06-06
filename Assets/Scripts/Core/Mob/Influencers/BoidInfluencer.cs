using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[RequireComponent(typeof(Boid))]
public class BoidInfluencer : MonoBehaviour
{
    private Boid boid;

    private void Awake()
    {
        boid = GetComponent<Boid>();

        // TODO: Remove this
        StartCoroutine(Coroutines.After(2f, () => boid.AttachToManager(BoidManager.Instance)));
    }

    public IEnumerable<Vector2> GetInfluences()
    {
        var forces = boid.ComputeForces();
        yield return forces.alignmentForce;
        yield return forces.cohesionForce;
        yield return forces.separationForce;
    }
}
