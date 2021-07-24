using System;
using static WorldGrid;

public class FloorDescriptor : Descriptor
{
    public enum Location
    {
        ROOM, HALLWAY
    }

    public readonly Location FloorLocation;

    public FloorDescriptor(Location floorLocation)
    {
        FloorLocation = floorLocation;
    }
}
