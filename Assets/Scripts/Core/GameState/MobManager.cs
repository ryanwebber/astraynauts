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

    [Header("Temporary")]

    [SerializeField]
    private MobInitializable mobPrefab;

    [SerializeField]
    private AnimationCurve spawnDistanceProbabilityCurve;

    public Event OnAllMobsDefeated;
    public Event<Mob> OnMobDefeated;
    public Event<Mob> OnMobWillSpawn;

    private HashSet<Mob> aliveMobs;

    private void Awake()
    {
        aliveMobs = new HashSet<Mob>();
        gameState.OnGameStateInitializationEnd += () =>
        {
            // TODO: Real mob spawning
            StartCoroutine(TestMobSpawning());
        };
    }

    private IEnumerator TestMobSpawning()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);
            if (TryFindSpawnableTeleporter(out var teleporter))
            {
                SpawnMobDelayed(mobPrefab, teleporter, 5f);
            }
        }
    }

    private bool TryFindSpawnableTeleporter(out Teleporter teleporter)
    {
        var playerPosition = gameState.Services.PlayerManager.ApproximatePlayerPositioning;

        // Elements at the back of this array are closer to the player, so the animation
        // curve should slope upwards.
        var sortedAvailableTeleporters = gameState.World.State.AccessibleTeleporters
            .OrderBy(teleporter => Vector2.Distance(playerPosition, teleporter.Center))
            .Reverse()
            .ToArray();

        if (sortedAvailableTeleporters.Length == 0)
        {
            teleporter = null;
            return false;
        }

        // Don't actually use the animation curve if there are only a couple
        // teleporters, as is the case in the early game
        var randomUnit = sortedAvailableTeleporters.Length > 4
            ? spawnDistanceProbabilityCurve.Evaluate(Random.value)
            : Random.value;

        var randomIndex = Mathf.FloorToInt(randomUnit * sortedAvailableTeleporters.Length);
        teleporter = sortedAvailableTeleporters[randomIndex];
        return true;
    }

    private void SpawnMobDelayed(MobInitializable prefab, Teleporter teleporter, float delay)
    {
        var instance = mobSpawner.SpawnMob(prefab, teleporter.Center);
        instance.OnWillSpawnIntoWorld?.Invoke();
        instance.OnMobDefeated += () => HandleMobDefeated(instance);
        aliveMobs.Add(instance);

        OnMobWillSpawn?.Invoke(instance);

        StartCoroutine(Coroutines.After(delay, () =>
        {
            instance.OnDidSpawnIntoWorld?.Invoke();
        }));
    }

    private void HandleMobDefeated(Mob mob)
    {
        OnMobDefeated?.Invoke(mob);

        if (aliveMobs.Contains(mob))
        {
            aliveMobs.Remove(mob);
            if (aliveMobs.Count == 0)
                OnAllMobsDefeated?.Invoke();
        }
        else
        {
            Debug.LogWarning("Defeated mob is not tracked by mob manager");
        }
    }
}
