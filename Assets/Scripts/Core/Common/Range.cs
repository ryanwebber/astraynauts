using System;
using UnityEngine;

[System.Serializable]
public struct Range
{
    public float min;
    public float max;

    public float Size => max - min;

    public Range(float min, float max)
    {
        this.min = min;
        this.max = max;
    }

    public float RandomInRange() => UnityEngine.Random.Range(min, max);
    public float Resolve(UnitScalar us) => min + Size * us;
}
