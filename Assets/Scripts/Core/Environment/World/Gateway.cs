using System;

public class Gateway
{
    public readonly Room Room;
    public readonly Hallway Hallway;
    public readonly Door Door;
    public readonly Direction OpeningDirection;

    public Gateway(Room room, Hallway hallway, Door door, Direction openingDirection)
    {
        Room = room;
        Hallway = hallway;
        Door = door;
        OpeningDirection = openingDirection;
    }
}
