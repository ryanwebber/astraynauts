using System;
using UnityEngine;

[System.Serializable]
public class WeightedInfluencer
{
    [SerializeField]
    private BaseInfluencer influencer;

    [SerializeField]
    private float weight = 1f;

    public WeightedInfluencer(BaseInfluencer influencer, float weight)
    {
        this.influencer = influencer;
        this.weight = weight;
    }

    public Vector2 GetWeightedInfluence()
    {
        Vector2 accumulatedInfluence = Vector2.zero;
        foreach (var influence in influencer.GetInfluences())
            accumulatedInfluence += influence;

        return accumulatedInfluence * weight;
    }
}
