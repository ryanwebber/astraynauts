using System;
using System.Collections;
using UnityEngine;

public struct Tween<T>
{
    private ITweenable<T> tweenable;
    private float duration;
    private ITweenCurve curve;

    public Tween(ITweenable<T> tweenable, float duration, ITweenCurve curve)
    {
        this.tweenable = tweenable;
        this.duration = duration;
        this.curve = curve;
    }

    public IEnumerator Bind(Action<T> action)
    {
        float time = 0f;
        while (time < duration)
        {
            float t0 = Mathf.InverseLerp(0, duration, time);
            float t1 = curve == null ? Mathf.Clamp01(t0) : curve.Invoke(t0);
            T value = tweenable.Interpolate(t1);
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
