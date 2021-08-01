using System;
using System.Collections.Generic;
using UnityEngine;
using static WorldGenerator;

public class CellMapping
{
    public readonly GeneratedRoomMapping Rooms;
    public readonly IReadOnlyList<GeneratedHallway> Hallways;
    public readonly IReadOnlyList<GeneratedAirlock> Airlocks;

    public readonly Parameters Parameters;

    public readonly IReadOnlyDictionary<Vector2Int, CellReference> Cells;

    public CellMapping(
        GeneratedRoomMapping rooms,
        IReadOnlyList<GeneratedHallway> hallways,
        IReadOnlyList<GeneratedAirlock> airlocks,
        Parameters parameters)
    {
        this.Rooms = rooms;
        this.Hallways = hallways;
        this.Airlocks = airlocks;
        this.Parameters = parameters;

        var mutableCells = new Dictionary<Vector2Int, CellReference>();
        foreach (var kv in rooms.Layout)
            mutableCells[kv.Key] = CellReference.FromRoom(kv.Value);

        foreach (var hallway in hallways)
            foreach (var cell in hallway.Path)
                mutableCells[cell] = CellReference.FromHallway(hallway);

        Cells = mutableCells;
    }
}