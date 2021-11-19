using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CleverCrow.Fluid.BTs.Trees;

[RequireComponent(typeof(Stage))]
public class SwarmMovementStage : MonoBehaviour
{
    [SerializeField]
    private GridLockedBody body;

    [SerializeField]
    private float movementDelay = 0.075f;

    [SerializeField]
    private MobInitializable initializer;

    [SerializeField]
    private List<WeightedInfluencer> influencers;

    private GridStepBehavior.Input input;
    private BehaviorTree behaviorTree;

    private void Awake()
    {
        var stage = GetComponent<Stage>();

        input = new GridStepBehavior.Input();
        behaviorTree = new BehaviorTreeBuilder(gameObject)
            .Sequence()
                .Condition(() => stage.IsStageActive)
                .Splice(new GridStepBehavior(body, input, movementDelay))
            .End()
            .Build();

        initializer.OnMobInitialize += (_, gs) => body.InitializeInWorld(gs.World);
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
