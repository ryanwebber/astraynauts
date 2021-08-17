using System;
using System.Collections.Generic;
using UnityEngine;

public class Hallway
{
    private readonly List<Gateway> gateways;
    public IReadOnlyList<Gateway> Gateways => gateways;

    public readonly IReadOnlyList<Vector2Int> Units;

    public bool IsSingleDoorHallway => gateways.Count == 2 && (gateways[0].Door == gateways[1].Door);

    public Hallway(IEnumerable<Vector2Int> units)
    {
        this.gateways = new List<Gateway>();
        this.Units = new List<Vector2Int>(units);
    }

    public void AddGateway(Gateway gateway)
    {
        gateways.Add(gateway);
    }

    public bool TryGetBinaryRoomConnection(out Door door)
    {
        if (IsSingleDoorHallway)
        {
            door = gateways[0].Door;
            return true;
        }

        door = default;
        return false;
    }

    public bool TryGetBinaryRoomConnection(out Room from, out Room to, out Direction direction)
    {
        if (TryGetBinaryRoomConnection(out var door) && door.TryGetBinaryRoomConnection(out from, out to, out direction))
            return true;

        from = default;
        to = default;
        direction = default;

        return false;
    }
}
