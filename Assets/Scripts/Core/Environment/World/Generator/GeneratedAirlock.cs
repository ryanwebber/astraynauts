using System;
using UnityEngine;

public class GeneratedAirlock
{
    public readonly GeneratedRoom AttachedRoom;
    public Vector2Int Cell;
    public Vector2Int Direction;

    public GeneratedAirlock(GeneratedRoom attachedRoom, Vector2Int cell, Vector2Int direction)
    {
        AttachedRoom = attachedRoom;
        Cell = cell;
        Direction = direction;
    }
}
