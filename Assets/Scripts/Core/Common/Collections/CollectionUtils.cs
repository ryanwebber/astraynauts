using System;
using System.Collections.Generic;

public static class CollectionUtils
{
    public static IEnumerable<(T, T)> Pair<T>(IEnumerator<T> collection, bool close = false)
    {
        if (!collection.MoveNext())
            yield break;

        T start = collection.Current;
        T current = start;
        int count = 1;
        while (collection.MoveNext())
        {
            T next = collection.Current;
            yield return (current, next);
            current = next;
            count++;
        }

        if (count >= 2 && close)
            yield return (current, start);
    }

    public static IEnumerable<(T, T)> Pair<T>(IEnumerable<T> collection, bool close = false)
    {
        foreach (var pair in Pair(collection.GetEnumerator(), close))
            yield return pair;
    }
}
