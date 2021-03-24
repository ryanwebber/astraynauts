using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TerrainLayer: IMutableTerrainLayer
{
    private Tilemap tilemap;
    public Tilemap Tilemap => tilemap;

    public TerrainLayer(Tilemap tilemap)
    {
        this.tilemap = tilemap;
    }
}
