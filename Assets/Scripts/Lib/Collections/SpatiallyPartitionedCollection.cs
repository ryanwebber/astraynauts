using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpaciallyPartitionedCollection<TElement, THashable> : IReadOnlyCollection<TElement>
{
    private Dictionary<TElement, int> bucketLookup;
    private HashSet<TElement>[] partitions;
    private System.Func<THashable, int> hashFn;

    public int Count => bucketLookup.Count;

    public SpaciallyPartitionedCollection(int nBuckets, System.Func<THashable, int> hashFn)
    {
        this.bucketLookup = new Dictionary<TElement, int>();
        this.partitions = new HashSet<TElement>[nBuckets];
        this.hashFn = h => hashFn.Invoke(h) % nBuckets;
    }

    public void AddOrUpdate(TElement element, THashable hashable)
    {
        int destBucket = hashFn.Invoke(hashable);
        if (bucketLookup.TryGetValue(element, out var bucket))
        {
            if (destBucket == bucket)
                return;

            partitions[bucket].Remove(element);
        }

        if (partitions[destBucket] == null)
            partitions[destBucket] = new HashSet<TElement>();

        partitions[destBucket].Add(element);
        bucketLookup[element] = destBucket;
    }

    public void Remove(TElement element)
    {
        if (bucketLookup.TryGetValue(element, out var bucket))
        {
            bucketLookup.Remove(element);
            partitions[bucket].Remove(element);
        }
    }

    public IReadOnlyCollection<TElement> GetBucket(THashable hash)
    {
        var bucket = hashFn.Invoke(hash);
        if (partitions[bucket] == null)
            return new TElement[] { };

        return partitions[bucket];
    }

    public IEnumerator<TElement> GetEnumerator()
    {
        return bucketLookup.Keys.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return (IEnumerator)GetEnumerator();
    }
}

