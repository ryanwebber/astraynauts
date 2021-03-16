using System;

public interface ITweenable<T>
{
    public T Interpolate(float t);
}
