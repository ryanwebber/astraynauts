using System;
using UnityEngine;

public delegate float ITweenCurve(float value);

public static class TweenCurve
{
    public static ITweenCurve Linear = LinearCurve;
    private static float LinearCurve(float value) => value;
}
