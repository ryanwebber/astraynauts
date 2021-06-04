using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(LocomotableActor))]
[RequireComponent(typeof(LocomotableInput))]
[RequireComponent(typeof(BoidInfluencer))]
[RequireComponent(typeof(CollisionAvoidanceInfluencer))]
[RequireComponent(typeof(ForwardMomentumInfluencer))]
[RequireComponent(typeof(NavigationTopologyInfluencer))]
public class SwarmMovementController : MonoBehaviour
{
    private LocomotableInput input;
    private BoidInfluencer boidInfluencer;
    private CollisionAvoidanceInfluencer collisionAvoidanceInfluencer;
    private ForwardMomentumInfluencer forwardMomentumInfluencer;
    private NavigationTopologyInfluencer navigationTopologyInfluencer;

    private void Awake()
    {
        input = GetComponent<LocomotableInput>();
        boidInfluencer = GetComponent<BoidInfluencer>();
        collisionAvoidanceInfluencer = GetComponent<CollisionAvoidanceInfluencer>();
        forwardMomentumInfluencer = GetComponent<ForwardMomentumInfluencer>();
        navigationTopologyInfluencer = GetComponent<NavigationTopologyInfluencer>();
    }

    private void Update()
    {
        input.MovementDirection = GetInfluence();
    }

    private Vector2 GetInfluence()
    {
        return new IEnumerable<Vector2>[]
            {
                boidInfluencer.GetInfluences(),
                collisionAvoidanceInfluencer.GetInfluences(),
                forwardMomentumInfluencer.GetInfluences(),
                navigationTopologyInfluencer.GetInfluences(),
            }
            .SelectMany(i => i)
            .Aggregate(Vector2.zero, (accum, influence) => accum + influence);
    }
}
