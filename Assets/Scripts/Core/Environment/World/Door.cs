using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class Door: ComponentMarker
{
    private readonly List<Gateway> gateways;
    public IReadOnlyList<Gateway> Gateways => gateways;

    public bool IsOpen { get; set; }
    public bool IsBinaryRoomConnection => gateways.Count == 2;

    public Door()
    {
        this.gateways = new List<Gateway>();
    }

    public void AddGateway(Gateway gateway)
    {
        // A door can have max 2 gateways, if it directly connects multiple rooms
        Assert.IsTrue(gateways.Count < 2);
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
}
