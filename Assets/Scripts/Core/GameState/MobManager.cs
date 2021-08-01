using UnityEngine;
using System.Collections;

public class MobManager : MonoBehaviour
{
    [SerializeField]
    private GameState gameState;

    [SerializeField]
    private NavigationService navigationService;
    public NavigationService NavigationService => navigationService;

    [SerializeField]
    private BoidManager boidManager;
    public BoidManager BoidManager => boidManager;

    [SerializeField]
    private MobSpawner mobSpawner;
    public MobSpawner MobSpawner => mobSpawner;

    [Header("Temporary")]

    [SerializeField]
    private MobInitializable mobPrefab;

    [SerializeField]
    private int numMobsToSpawn;

    private void Awake()
    {
        gameState.OnGameStateInitializationEnd += () =>
        {
            // TODO: Real mob spawning
            for (int i = 0; i < numMobsToSpawn; i ++)
            {
                var randomRoomIndex = Random.Range(0, gameState.World.CellLayout.Rooms.AllRooms.Count);
                var room = gameState.World.CellLayout.Rooms.AllRooms[randomRoomIndex];
                var sectionIndex = Random.Range(0, room.SectionCount);
                var section = room.GetSection(sectionIndex);
                var spawnPosition = gameState.World.CellToWorldPosition(section.center);
                mobSpawner.SpawnMob(mobPrefab, spawnPosition);
            }
        };
    }
}
