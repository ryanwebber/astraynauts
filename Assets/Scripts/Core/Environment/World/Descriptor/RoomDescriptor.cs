using System;

public class RoomDescriptor: WorldGrid.Descriptor
{
    public readonly Room Room;

    public RoomDescriptor(Room room)
    {
        Room = room;
    }
}
