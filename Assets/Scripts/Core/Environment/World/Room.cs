using System;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public readonly IReadOnlyList<Vector2Int> Units;

    public Room(IEnumerable<Vector2Int> units)
    {
        Units = new List<Vector2Int>(units);
    }
}
