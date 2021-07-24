using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Operator
{
    AND, OR
}

public interface ITileCondition
{
    bool Evaluate(WorldGrid grid, Vector2Int position);
}

public struct RelativeTileTypeCondition<T>: ITileCondition where T: WorldGrid.Descriptor
{
    public Vector2Int offset;
    public Func<T, bool> predicate;

    public bool Evaluate(WorldGrid grid, Vector2Int position)
    {
        if (grid.TryGetDescriptor<T>(position + offset, out var descriptor))
            if (predicate == null || predicate.Invoke(descriptor))
                return true;

        return false;
    }
}

public struct CompositeTileCondition : ITileCondition
{
    private ICollection<ITileCondition> conditions;
    private Operator op;

    public CompositeTileCondition(params ITileCondition[] conditions)
    {
        this.op = Operator.AND;
        this.conditions = conditions;
    }

    public CompositeTileCondition(Operator op, params ITileCondition[] conditions)
    {
        this.op = op;
        this.conditions = conditions;
    }

    public bool Evaluate(WorldGrid grid, Vector2Int position)
    {
        if (op == Operator.AND)
            return conditions.All(condition => condition.Evaluate(grid, position));
        else
            return conditions.Any(condition => condition.Evaluate(grid, position));
    }
}

public struct NegatedCompositeCondition: ITileCondition
{
    private ITileCondition condition;

    public NegatedCompositeCondition(ITileCondition condition)
    {
        this.condition = condition;
    }

    public bool Evaluate(WorldGrid grid, Vector2Int position) => !condition.Evaluate(grid, position);
}
