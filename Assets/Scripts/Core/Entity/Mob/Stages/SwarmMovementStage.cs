using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CleverCrow.Fluid.BTs.Trees;

[RequireComponent(typeof(Stage))]
public class SwarmMovementStage : MonoBehaviour
{
    [SerializeField]
    private KinematicBody kinematicBody;

    [SerializeField]
    private WalkBehavior.Properties properties;

    [SerializeField]
    private List<WeightedInfluencer> influencers;

    private WalkBehavior.Input input;
    private BehaviorTree behaviorTree;

    private void Awake()
    {
        var stage = GetComponent<Stage>();

        input = new WalkBehavior.Input();
        behaviorTree = new BehaviorTreeBuilder(gameObject)
            .Sequence()
                .Condition(() => stage.IsStageActive)
                .Splice(new WalkBehavior(kinematicBody, input, properties))
            .End()
            .Build();
    }

    private void Update()
    {
        input.Direction = GetInfluence();
        behaviorTree.Tick();
    }

    private Vector2 GetInfluence()
    {
        Vector2 influence = Vector2.zero;
        foreach (var influencer in influencers)
            influence += influencer.GetWeightedInfluence();

        return influence.normalized;
    }
}
