using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    [SerializeField]
    private GameState gameState;

    [Header("Temporary")]

    [SerializeField]
    private MobInitializable mob;

    [SerializeField]
    private int batchesInWave;

    [SerializeField]
    private int mobsInBatch;

    [SerializeField]
    private int maxAliveMobs;

    [SerializeField]
    private float mobSpawnDelay;

    private MobManager MobManager => gameState.Services.MobManager;

    private void Awake()
    {
        gameState.OnGameStateInitializationEnd += () =>
        {
            StartCoroutine(Coroutines.After(3f, () => SpawnWave()));
        };

        MobManager.OnMobDefeated += _ =>
        {
            if (MobManager.AliveMobCount == 0 && MobManager.EnqueuedBatchCount == 0)
            {
                Debug.Log("Wave cleared!");
                SpawnWave();
            }
        };
    }

    private void SpawnWave()
    {
        Debug.Log("Spawning wave!");
        for (int i = 0; i < batchesInWave; i++)
        {
            var entry = new MobSpawnEntry
            {
                count = mobsInBatch,
                prefab = mob
            };

            var batch = new MobSpawnBatch(maxAliveMobs, mobSpawnDelay, new MobSpawnEntry[] { entry });
            batch.OnBatchSpawnBegin += () => Debug.Log("Spawning batch!");
            batch.OnBatchSpawnComplete += () => Debug.Log("Last mob in batch spawned!");

            MobManager.EnqueueBatch(batch);
        }
    }
}
