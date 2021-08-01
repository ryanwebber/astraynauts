using System;
using System.Collections.Generic;
using UnityEngine;

public class GeneratedRoomMapping
{
    private IReadOnlyList<GeneratedRoom> rooms;
    private IReadOnlyDictionary<Vector2Int, GeneratedRoom> layout;

    public GeneratedRoomMapping(IReadOnlyList<GeneratedRoom> rooms, IReadOnlyDictionary<Vector2Int, GeneratedRoom> layout)
    {
        this.rooms = rooms;
        this.layout = layout;
    }

    public IReadOnlyList<GeneratedRoom> AllRooms => rooms;
    public IReadOnlyDictionary<Vector2Int, GeneratedRoom> Layout => layout;
}
