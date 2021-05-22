using System;

public struct CellType
{
    private static Mask DOOR_MASK = new Mask(2);
    private static Mask HALL_MASK = new Mask(3);

    public static CellType Empty => new CellType(0);
    public static CellType Room => new CellType(1);
    public static CellType Door => new CellType(1 & DOOR_MASK.rawMask);
    public static CellType Hall => new CellType(1 & HALL_MASK.rawMask);

    private uint data;

    public bool IsEmpty => data == 0;
    public bool IsFloor => !IsEmpty;

    public bool IsDoor => IsFloor && DOOR_MASK.IsSet(data);
    public bool IsHall => IsFloor && HALL_MASK.IsSet(data);
    public bool IsRoom => data == 1;

    private CellType(uint rawValue)
    {
        this.data = rawValue;
    }
}
