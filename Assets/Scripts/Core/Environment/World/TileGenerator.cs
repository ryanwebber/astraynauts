using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public struct TileGenerator
{
    public ITileSource source;
    public Tilemap layer;

    public Vector2Int offset;

    public TileAssignment GetAssignment(Vector2Int position)
    {
        return new TileAssignment
        {
            tile = source.GetTile(),
            tilemap = layer,
            position = position + offset
        };
    }
}
