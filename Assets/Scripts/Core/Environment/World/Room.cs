using System;
using System.Collections.Generic;
using UnityEngine;

public class Room: ComponentMarker
{
    public readonly IReadOnlyList<Vector2Int> Units;
    public List<Teleporter> Teleporters;

    public Room(IEnumerable<Vector2Int> units)
    {
        Units = new List<Vector2Int>(units);
        Teleporters = new List<Teleporter>();
    }
}
