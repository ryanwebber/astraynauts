using System;

public class RoomDescriptor: WorldGrid.Descriptor, IComponentDescriptor<Room>
{
    public readonly Room Room;

    public RoomDescriptor(Room room)
    {
        Room = room;
    }

    Room IComponentDescriptor<Room>.Component => Room;
}
