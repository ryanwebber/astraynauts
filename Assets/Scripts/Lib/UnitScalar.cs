using System;
using UnityEngine;

[System.Serializable]
public struct UnitScalar
{
    [SerializeField]
    [Range(0f, 1f)]
    private float value;
    public float Value
    {
        get => value;
        set => this.value = Mathf.Clamp01(value);
    }

    public UnitScalar Inverse => new UnitScalar(1f - value);

    public UnitScalar(float f)
    {
        value = default;
        Value = f;
    }

    public static implicit operator UnitScalar(float f)
    {
        return new UnitScalar(f);
    }

    public static implicit operator float(UnitScalar us)
    {
        return us.value;
    }
}
