using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public interface IMutableTerrainLayer
{
    Tilemap Tilemap { get; }
}
