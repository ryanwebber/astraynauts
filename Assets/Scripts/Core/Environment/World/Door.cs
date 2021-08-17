using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class Door: ComponentMarker
{
    private readonly List<Gateway> gateways;
    public IReadOnlyList<Gateway> Gateways => gateways;

    public readonly Vector2 Center;

    public bool IsOpen { get; private set; }
    public bool IsBinaryRoomConnection => gateways.Count == 2;

    public bool IsHorizontal => gateways.Count == 0 ? false : gateways[0].OpeningDirection.IsHorizontal;
    public bool IsVertical => !IsHorizontal;

    public Door(Vector2 center)
    {
        this.gateways = new List<Gateway>(2);
        this.Center = center;
    }

    public void AddGateway(Gateway gateway)
    {
        // A door can have max 2 gateways, if it directly connects multiple rooms
        Assert.IsTrue(gateways.Count < 2);
        Assert.IsTrue(gateways.All(g => g.OpeningDirection.IsHorizontal == gateway.OpeningDirection.IsHorizontal));

        gateways.Add(gateway);
    }

    public bool TryGetBinaryRoomConnection(out Room from, out Room to, out Direction direction)
    {
        if (gateways.Count == 2)
        {
            from = gateways[0].Room;
            to = gateways[1].Room;
            direction = gateways[1].OpeningDirection;
            return true;
        }

        from = default;
        to = default;
        direction = default;

        return false;
    }

    public void Update(bool isOpen)
    {
        IsOpen = isOpen;
    }
}
