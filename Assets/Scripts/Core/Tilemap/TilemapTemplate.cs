using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapTemplate: IReadOnlyCollection<Vector3Int>
{
    private BoundsInt bounds;
    private Dictionary<Vector3Int, TileBase> data;

    public BoundsInt Bounds => bounds;

    public int Count => data.Count;

    private TilemapTemplate(Dictionary<Vector3Int, TileBase> data, BoundsInt bounds)
    {
        this.data = data;
        this.bounds = bounds;
    }

    public TileBase GetTile(Vector3Int cell)
    {
        return data[cell];
    }

    public TileBase GetTile(Vector2Int cell)
    {
        return data[(Vector3Int)cell];
    }

    public bool Contains(Vector3Int cell)
    {
        return data.ContainsKey(cell);
    }

    public bool Intersects(IEnumerable<Vector3Int> cells)
    {
        foreach (var value in cells)
        {
            if (Contains(value))
                return true;
        }

        return false;
    }

    public IEnumerator<Vector3Int> GetEnumerator()
    {
        return data.Keys.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return data.Keys.GetEnumerator();
    }

    public static TilemapTemplate From(Tilemap tilemap)
    {
        Dictionary<Vector3Int, TileBase> data = new Dictionary<Vector3Int, TileBase>();
        foreach (var position in tilemap.cellBounds.allPositionsWithin)
        {
            var tile = tilemap.GetTile(position);
            if (tile != null)
                data[position] = tile;
        }

        return new TilemapTemplate(data, tilemap.cellBounds);
    }
}
