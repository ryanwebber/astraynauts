using System;
using System.Collections.Generic;

public class MobSpawnBatch
{
    public struct MobIndex
    {
        public int currentIndex;
        public int countInBatch;
    }

    public Event OnBatchSpawnBegin;
    public Event OnBatchSpawnComplete;
    public Event<MobInitializable, MobIndex> OnSpawnTriggeredInBatch;

    public readonly int MaximumAliveMobs;
    public readonly float SpawnDelay;
    public readonly IReadOnlyList<MobSpawnEntry> SpawnEntries;

    public MobSpawnBatch(int maximumAliveMobs, float spawnDelay, IReadOnlyList<MobSpawnEntry> spawnEntries)
    {
        MaximumAliveMobs = maximumAliveMobs;
        SpawnDelay = spawnDelay;
        SpawnEntries = spawnEntries;
    }

    public static MobSpawnBatch SpawnOne(MobInitializable prefab)
    {
        var entry = new MobSpawnEntry
        {
            count = 1,
            prefab = prefab
        };

        return new MobSpawnBatch(int.MaxValue, 0f, new MobSpawnEntry[] { entry });
    }
}
