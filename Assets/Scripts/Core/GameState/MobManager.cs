using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MobManager : MonoBehaviour
{
    [SerializeField]
    private GameState gameState;

    [SerializeField]
    private BoidManager boidManager;
    public BoidManager BoidManager => boidManager;

    [SerializeField]
    private NavigationService navigationService;
    public NavigationService NavigationService => navigationService;

    [SerializeField]
    private MobSpawner mobSpawner;
    public MobSpawner MobSpawner => mobSpawner;

    [SerializeField]
    private AnimationCurve spawnDistanceProbabilityCurve;

    [SerializeField]
    private int maxTopTeleporterSelectionCount = 6;

    [SerializeField]
    private int minTeleporterPoolSizeForCurvedSelection = 4;

    [SerializeField]
    private float teleportTime = 2.5f;

    [SerializeField]
    private float spawnAttemptRefreshTime = 1f;

    [Header("Debug")]

    [SerializeField]
    private bool preventMobSpawning = false;

    public Event<Mob> OnMobDefeated;
    public Event<Mob> OnMobWillSpawn;

    public int AliveMobCount => aliveMobs.Count;
    public int EnqueuedBatchCount => spawnBatches.Count;

    private HashSet<Mob> aliveMobs;
    private Queue<MobSpawnBatch> spawnBatches;

    private void Awake()
    {
        aliveMobs = new HashSet<Mob>();
        spawnBatches = new Queue<MobSpawnBatch>();

        gameState.OnGameStateInitializationEnd += () =>
        {
            StartCoroutine(ContinuouslySpawnMobs());
        };
    }

    public void EnqueueBatch(MobSpawnBatch batch)
    {
        spawnBatches.Enqueue(batch);
    }

    private IEnumerator ContinuouslySpawnMobs()
    {
        IEnumerable<MobInitializable> ExpandBatch(MobSpawnEntry entry)
        {
            for (int i = 0; i < entry.count; i++)
                yield return entry.prefab;
        }

        while (true)
        {
            if (spawnBatches.Count > 0 && !preventMobSpawning)
            {
                var currentBatch = spawnBatches.Dequeue();
                var mobsToSpawn = currentBatch.SpawnEntries.SelectMany(ExpandBatch);
                var numMobsToSpawn = currentBatch.SpawnEntries.Sum(entry => entry.count);
                var currentIndex = 0;

                currentBatch.OnBatchSpawnBegin?.Invoke();

                foreach (var mob in mobsToSpawn)
                {
                    // Wait until there are few enough mobs to spawn one
                    while (AliveMobCount >= currentBatch.MaximumAliveMobs)
                        yield return new WaitForSeconds(spawnAttemptRefreshTime);

                    if (TryFindSpawnableTeleporter(out var teleporter))
                    {
                        SpawnMobDelayed(mob, teleporter, teleportTime);
                        currentBatch.OnSpawnTriggeredInBatch?.Invoke(mob, new MobSpawnBatch.MobIndex
                        {
                            currentIndex = currentIndex,
                            countInBatch = numMobsToSpawn,
                        });

                        yield return new WaitForSeconds(currentBatch.SpawnDelay);
                    }

                    currentIndex++;
                }

                currentBatch.OnBatchSpawnComplete?.Invoke();
            }

            yield return new WaitForSeconds(spawnAttemptRefreshTime);
        }
    }

    private bool TryFindSpawnableTeleporter(out Teleporter teleporter)
    {
        var playerPosition = gameState.Services.PlayerManager.ApproximatePlayerPositioning;

        // Elements at the back of this array are closer to the player, so the animation
        // curve should slope upwards.
        var sortedAvailableTeleporters = gameState.World.State.AccessibleTeleporters
            .OrderBy(teleporter => Vector2.Distance(playerPosition, teleporter.Center))
            .Take(maxTopTeleporterSelectionCount)
            .Reverse()
            .ToArray();

        if (sortedAvailableTeleporters.Length == 0)
        {
            teleporter = null;
            return false;
        }

        // Don't actually use the animation curve if there are only a couple
        // teleporters, as is the case in the early game
        var randomUnit = sortedAvailableTeleporters.Length > minTeleporterPoolSizeForCurvedSelection
            ? spawnDistanceProbabilityCurve.Evaluate(Random.value)
            : Random.value;

        var randomIndex = Mathf.FloorToInt(randomUnit * sortedAvailableTeleporters.Length);
        teleporter = sortedAvailableTeleporters[randomIndex];
        return true;
    }

    private void SpawnMobDelayed(MobInitializable prefab, Teleporter teleporter, float delay)
    {
        var instance = mobSpawner.SpawnMob(prefab, teleporter.Center);
        instance.OnMobDefeated += () => HandleMobDefeated(instance);

        aliveMobs.Add(instance);
        OnMobWillSpawn?.Invoke(instance);

        instance.OnMobSpawn?.Invoke();
    }

    private void HandleMobDefeated(Mob mob)
    {
        if (aliveMobs.Contains(mob))
        {
            aliveMobs.Remove(mob);
            OnMobDefeated?.Invoke(mob);
        }
        else
        {
            Debug.LogWarning("Defeated mob is not tracked by mob manager", mob);
        }
    }
}
