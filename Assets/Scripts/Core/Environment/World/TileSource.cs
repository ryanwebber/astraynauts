using System;
using UnityEngine.Tilemaps;

public interface ITileSource
{
    public TileBase GetTile();
}

public struct SingleTile : ITileSource
{
    public TileBase tile;

    public SingleTile(TileBase tile)
    {
        this.tile = tile;
    }

    public TileBase GetTile() => tile;
}

public struct RandomTile : ITileSource
{
    public IRandomAccessCollection<TileBase> tiles;

    public RandomTile(IRandomAccessCollection<TileBase> tiles)
    {
        this.tiles = tiles;
    }

    public TileBase GetTile() => tiles.NextValue();
}
