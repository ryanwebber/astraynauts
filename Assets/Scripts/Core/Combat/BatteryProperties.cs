using System;
using UnityEngine;

[System.Serializable]
public struct BatteryProperties : IEquatable<BatteryProperties>
{
    [SerializeField]
    [Min(0)]
    private int maxCharge;
    public int MaxCharge
    {
        get => maxCharge;
        set => maxCharge = Mathf.Max(0, value);
    }

    public override bool Equals(object obj)
    {
        return obj is BatteryProperties properties && Equals(properties);
    }

    public bool Equals(BatteryProperties other)
    {
        return maxCharge == other.maxCharge;
    }

    public override int GetHashCode()
    {
        return -1868854947 + maxCharge.GetHashCode();
    }

    public static bool operator ==(BatteryProperties left, BatteryProperties right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(BatteryProperties left, BatteryProperties right)
    {
        return !(left == right);
    }
}
