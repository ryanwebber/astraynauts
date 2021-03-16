using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Tween<T>
{
    private ITweenable<T> tweenable;
    private float duration;
    private AnimationCurve curve;

    public Tween(ITweenable<T> tweenable, float duration, AnimationCurve curve)
    {
        this.tweenable = tweenable;
        this.duration = duration;
        this.curve = curve;
    }

    public IEnumerable Bind(System.Action<T> action)
    {
        float time = 0f;
        while (time < duration)
        {
            float t = curve.Evaluate(Mathf.InverseLerp(0, duration, time));
            T value = tweenable.Interpolate(t);
            action(value);
            yield return 0;
        }

        action(tweenable.Interpolate(1f));
    }
}
