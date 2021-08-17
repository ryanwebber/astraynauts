using System;

public class HallwayDescriptor: WorldGrid.Descriptor
{
    public readonly Hallway Hallway;

    public HallwayDescriptor(Hallway hallway)
    {
        Hallway = hallway;
    }
}
