using System;

public struct Mask
{
    public uint rawMask;

    public Mask(int bit)
    {
        rawMask = 1u << bit;
    }

    public bool IsSet(uint value)
    {
        return (value & rawMask) != 0;
    }
}
