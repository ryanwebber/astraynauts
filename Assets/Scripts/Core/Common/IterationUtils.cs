using System;

public static class IterationUtils
{
    public static bool TryUntil<T>(out T result, int maxIterations, Func<int, T> producer) where T: class
    {
        for (int i = 0; i < maxIterations; i++)
        {
            result = producer?.Invoke(i);
            if (result != null)
                return true;
        }

        result = default;
        return false;
    }

    public static T TryUntil<T>(int maxIterations, Func<int, T> producer) where T : class
    {
        if (TryUntil(out var t, maxIterations, producer))
            return t;

        return null;
    }
}
