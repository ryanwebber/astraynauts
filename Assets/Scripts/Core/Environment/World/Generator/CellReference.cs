using System;
using UnityEngine.Assertions;

public class CellReference
{
    public enum CellType
    {
        ROOM, HALLWAY, AIRLOCK
    }

    public readonly CellType Type;

    private GeneratedHallway hallway;
    public GeneratedHallway AsHallway
    {
        get
        {
            Assert.AreEqual(Type, CellType.HALLWAY);
            return hallway;
        }
    }

    private GeneratedRoom room;
    public GeneratedRoom AsRoom
    {
        get
        {
            Assert.AreEqual(Type, CellType.ROOM);
            return room;
        }
    }

    private GeneratedAirlock airlock;
    public GeneratedAirlock AsAirlock
    {
        get
        {
            Assert.AreEqual(Type, CellType.AIRLOCK);
            return airlock;
        }
    }

    private CellReference(CellType type, GeneratedHallway hallway = null, GeneratedRoom room = null, GeneratedAirlock airlock = null)
    {
        this.Type = type;
        this.hallway = hallway;
        this.room = room;
        this.airlock = airlock;
    }

    public static CellReference FromHallway(GeneratedHallway hallway) => new CellReference(CellType.HALLWAY, hallway: hallway);
    public static CellReference FromRoom(GeneratedRoom room) => new CellReference(CellType.ROOM, room: room);
    public static CellReference FromAirlock(GeneratedAirlock airlock) => new CellReference(CellType.AIRLOCK, airlock: airlock);
}
