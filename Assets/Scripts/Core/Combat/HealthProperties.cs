using UnityEngine;
using System.Collections;
using System;

[System.Serializable]
public struct HealthProperties : IEquatable<HealthProperties>
{
    [SerializeField]
    [Min(1)]
    private int maxHealth;
    public int MaxHealth
    {
        get => maxHealth;
        set => maxHealth = Mathf.Max(1, value);
    }

    public override bool Equals(object obj)
    {
        return obj is HealthProperties properties && Equals(properties);
    }

    public bool Equals(HealthProperties other)
    {
        return maxHealth == other.maxHealth &&
               MaxHealth == other.MaxHealth;
    }

    public override int GetHashCode()
    {
        int hashCode = -1563151346;
        hashCode = hashCode * -1521134295 + maxHealth.GetHashCode();
        hashCode = hashCode * -1521134295 + MaxHealth.GetHashCode();
        return hashCode;
    }

    public static bool operator ==(HealthProperties left, HealthProperties right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(HealthProperties left, HealthProperties right)
    {
        return !(left == right);
    }
}
