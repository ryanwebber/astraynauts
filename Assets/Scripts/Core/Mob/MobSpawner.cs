using UnityEngine;
using System.Collections;

public class MobSpawner : MonoBehaviour
{
    [SerializeField]
    private Transform spawnParent;

    [SerializeField]
    private GameState gameState;

    public Mob SpawnMob(MobInitializable prefab, Vector2 position)
    {
        var instance = Instantiate(prefab, spawnParent);
        instance.transform.position = position;
        return instance.Initialize(gameState);
    }
}
