using System;
using System.Collections.Generic;
using UnityEngine;

public struct TileRulePair
{
    public ITileCondition condition;
    public TileGenerator[] generators;

    public IEnumerable<TileAssignment> GetAssignments(WorldGrid grid, Vector2Int anchor)
    {
        if (generators == null || generators.Length == 0 || !condition.Evaluate(grid, anchor))
            yield break;

        foreach (var generator in generators)
        {
            if (generator.layer == null || generator.source == null)
                continue;

            yield return generator.GetAssignment(anchor);
        }
    }
}
