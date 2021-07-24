using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public struct TileAssignment : IOperation
{
    public TileBase tile;
    public Vector2Int position;
    public Tilemap tilemap;

    public void Perform() => tilemap?.SetTile((Vector3Int)position, tile);
}
