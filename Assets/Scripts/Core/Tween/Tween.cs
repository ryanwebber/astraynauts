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

    public IEnumerator Bind(System.Action<T> action)
    {
        float time = 0f;
        while (time < duration)
        {
            float t = curve.Evaluate(Mathf.InverseLerp(0, duration, time));
            T value = tweenable.Interpolate(t);
            action(value);
            yield return null;

            time += Time.deltaTime;
        }

        action(tweenable.Interpolate(1f));
    }

    public Tween<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        var tweenable = new MapTweenable<TNew>
        {
            tweenable = this.tweenable,
            mapper = mapper
        };

        return new Tween<TNew>(tweenable, duration, curve);
    }

    private struct MapTweenable<TNew> : ITweenable<TNew>
    {
        public ITweenable<T> tweenable;
        public Func<T, TNew> mapper;

        public TNew Interpolate(float t)
        {
            return mapper.Invoke(tweenable.Interpolate(t));
        }
    }
}
