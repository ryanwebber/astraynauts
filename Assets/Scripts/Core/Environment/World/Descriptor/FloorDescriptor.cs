using System;
using static WorldGrid;

public class FloorDescriptor : Descriptor
{
    public enum Location
    {
        ROOM, HALLWAY, TELEPORTER
    }

    public readonly Location FloorLocation;

    public FloorDescriptor(Location floorLocation)
    {
        FloorLocation = floorLocation;
    }

    public static Func<FloorDescriptor, bool> IsA(Location location)
    {
        return d => d.FloorLocation == location;
    }
}
