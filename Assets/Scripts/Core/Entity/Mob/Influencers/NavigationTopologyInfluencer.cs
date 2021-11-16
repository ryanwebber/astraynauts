using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NavigationTopologyInfluencer : BaseInfluencer
{
    [SerializeField]
    private MobInitializable initializer;

    private NavigationTopology topology;

    private void Awake()
    {
        initializer.OnMobInitialize += (_, ctx) => topology = ctx.Services.MobManager.NavigationService.NavigationTopology;
    }

    public override IEnumerable<Vector2> GetInfluences()
    {
        if (topology == null || !topology.IsInitialized)
            yield break;

        var cell = new Vector2Int(
            Mathf.FloorToInt(transform.position.x),
            Mathf.FloorToInt(transform.position.y)
        );

        var slope = topology.GetTopology(cell).slope;
        foreach (var dir in Direction.Neighboring)
        {
            slope += topology.GetTopology(cell + dir).slope;
        }

        yield return slope.normalized;
    }
}
