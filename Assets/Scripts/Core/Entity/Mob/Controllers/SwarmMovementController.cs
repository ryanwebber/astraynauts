using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CleverCrow.Fluid.BTs.Trees;

[RequireComponent(typeof(BoidInfluencer))]
[RequireComponent(typeof(CollisionAvoidanceInfluencer))]
[RequireComponent(typeof(ForwardMomentumInfluencer))]
[RequireComponent(typeof(NavigationTopologyInfluencer))]
public class SwarmMovementController : MonoBehaviour
{
    [SerializeField]
    private KinematicBody kinematicBody;

    [SerializeField]
    private WalkBehavior.Properties properties;

    private BoidInfluencer boidInfluencer;
    private CollisionAvoidanceInfluencer collisionAvoidanceInfluencer;
    private ForwardMomentumInfluencer forwardMomentumInfluencer;
    private NavigationTopologyInfluencer navigationTopologyInfluencer;

    private WalkBehavior.Input input;
    private BehaviorTree behaviorTree;

    private void Awake()
    {
        boidInfluencer = GetComponent<BoidInfluencer>();
        collisionAvoidanceInfluencer = GetComponent<CollisionAvoidanceInfluencer>();
        forwardMomentumInfluencer = GetComponent<ForwardMomentumInfluencer>();
        navigationTopologyInfluencer = GetComponent<NavigationTopologyInfluencer>();

        input = new WalkBehavior.Input();

        behaviorTree = new WalkBehavior(kinematicBody, input, properties)
            .ToBehaviorTree(gameObject);
    }

    private void Update()
    {
        input.Direction = GetInfluence();
        behaviorTree.Tick();
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
