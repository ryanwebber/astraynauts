using System;
using UnityEngine;

[System.Serializable]
public struct RangeInt
{
    public int min;
    public int max;

    public int Size => max - min;

    public RangeInt(int min, int max)
    {
        this.min = min;
        this.max = max;
    }

    public int RandomInRange() => UnityEngine.Random.Range(min, max);

    public int Resolve(UnitScalar us)
    {
        return min + Mathf.FloorToInt(Size * us);
    }
}
