using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpaciallyPartitionedCollection<TKey>: IReadOnlyCollection<SpaciallyPartitionedCollection<TKey>.Element>
{
    public struct Element
    {
        public TKey entity;
        public Vector2 position;
    }

    public readonly Vector2Int Dimensions;
    public readonly Vector2 CellSize;

    private Dictionary<TKey, Vector2>[,] partition;
    private Dictionary<TKey, Vector2Int> cellLookup;

    public SpaciallyPartitionedCollection(Vector2Int dimensions, Vector2 cellSize)
    {
        Dimensions = dimensions;
        CellSize = cellSize;

        partition = new Dictionary<TKey, Vector2>[dimensions.x, dimensions.y];
        cellLookup = new Dictionary<TKey, Vector2Int>();
    }

    public int Count => cellLookup.Count;

    public bool IsCellInBounds(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < Dimensions.x &&
            cell.y >= 0 && cell.y < Dimensions.y;
    }

    public IEnumerable<Element> GetEntitiesInCell(Vector2Int cell)
    {
        if (!IsCellInBounds(cell) || partition[cell.x, cell.y] == null)
            return Enumerable.Empty<Element>();

        return partition[cell.x, cell.y]
            .Select(kvp => new Element { entity = kvp.Key, position = kvp.Value });
    }

    public bool TryGetCell(TKey entity, out Vector2Int cell)
    {
        return cellLookup.TryGetValue(entity, out cell);
    }

    public bool TryGetPosition(TKey entity, out Vector2 position)
    {
        if (TryGetCell(entity, out var cell))
        {
            position = partition[cell.x, cell.y][entity];
            return true;
        }

        position = default;
        return false;
    }

    public bool Contains(TKey entity)
    {
        return TryGetPosition(entity, out var _);
    }

    public bool Remove(TKey entity)
    {
        if (TryGetCell(entity, out var cell))
        {
            partition[cell.x, cell.y].Remove(entity);
            cellLookup.Remove(entity);
            return true;
        }

        return false;
    }

    public Vector2Int PositionToCell(Vector2 position)
    {
        int x = Mathf.FloorToInt(position.x / CellSize.x);
        int y = Mathf.FloorToInt(position.y / CellSize.y);
        return new Vector2Int(x, y);
    }

    public IEnumerator<Element> GetEnumerator()
    {
        foreach (var e in cellLookup)
        {
            if (partition[e.Value.x, e.Value.y] == null)
            {
                Debug.LogWarning($"Null partition: {e.Value}");
            }
        }

        var enumerable = cellLookup.Select(kvp => new Element
        {
            entity = kvp.Key,
            position = partition[kvp.Value.x, kvp.Value.y][kvp.Key]
        });

        return enumerable.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return (IEnumerator) this.GetEnumerator();
    }

    public Vector2 this[TKey entity]
    {
        get
        {
            if (TryGetPosition(entity, out var position))
                return position;

            throw new System.Exception("Unknown entity");
        }

        set
        {
            var targetCell = PositionToCell(value);
            if (!IsCellInBounds(targetCell))
            {
                Remove(entity);
                return;
            }

            if (TryGetCell(entity, out var cell) && targetCell != cell)
                partition[cell.x, cell.y].Remove(entity);

            if (partition[targetCell.x, targetCell.y] == null)
                partition[targetCell.x, targetCell.y] = new Dictionary<TKey, Vector2>();

            partition[targetCell.x, targetCell.y][entity] = value;
            cellLookup[entity] = targetCell;
        }
    }
}
