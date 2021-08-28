using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(ComponentBehavior))]
[RequireComponent(typeof(BoidInfluencer))]
[RequireComponent(typeof(CollisionAvoidanceInfluencer))]
[RequireComponent(typeof(ForwardMomentumInfluencer))]
[RequireComponent(typeof(NavigationTopologyInfluencer))]
public class SwarmMovementController : MonoBehaviour, IActivatable
{
    [SerializeField]
    private WalkingInput input;

    [SerializeField]
    private WalkingActor actor;

    private BoidInfluencer boidInfluencer;
    private CollisionAvoidanceInfluencer collisionAvoidanceInfluencer;
    private ForwardMomentumInfluencer forwardMomentumInfluencer;
    private NavigationTopologyInfluencer navigationTopologyInfluencer;

    [SerializeField]
    private bool isActive = false;
    public bool IsActive
    {
        get => isActive;
        set => isActive = value;
    }

    private void Awake()
    {
        boidInfluencer = GetComponent<BoidInfluencer>();
        collisionAvoidanceInfluencer = GetComponent<CollisionAvoidanceInfluencer>();
        forwardMomentumInfluencer = GetComponent<ForwardMomentumInfluencer>();
        navigationTopologyInfluencer = GetComponent<NavigationTopologyInfluencer>();

        GetComponent<ComponentBehavior>()
            .Bind(this)
            .Bind(actor);
    }

    private void Update()
    {
        if (IsActive)
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
