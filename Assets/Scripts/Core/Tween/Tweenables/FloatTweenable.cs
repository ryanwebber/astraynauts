using UnityEngine;

public struct FloatTweenable : ITweenable<float>
{
    public float from;
    public float to;

    public FloatTweenable(float from, float to)
    {
        this.from = from;
        this.to = to;
    }

    public float Interpolate(float t)
    {
        return Mathf.LerpUnclamped(from, to, t);
    }
}
