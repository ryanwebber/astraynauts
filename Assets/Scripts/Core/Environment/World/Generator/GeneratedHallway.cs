using System;
using System.Collections.Generic;
using UnityEngine;

public class GeneratedHallway
{
    private HashSet<Vector2Int> path;
    private IReadOnlyDictionary<GeneratedRoom, Vector2Int> doorMapping;

    public GeneratedHallway(HashSet<Vector2Int> path, IReadOnlyDictionary<GeneratedRoom, Vector2Int> doorMapping)
    {
        this.path = path;
        this.doorMapping = doorMapping;
    }

    public IEnumerable<Vector2Int> GetCells() => path;
    public IReadOnlyDictionary<GeneratedRoom, Vector2Int> DoorMapping => doorMapping;
    public ISet<Vector2Int> Path => path;
}
