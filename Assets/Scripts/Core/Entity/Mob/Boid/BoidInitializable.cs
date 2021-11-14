using UnityEngine;
using System.Collections;

public class BoidInitializable : MonoBehaviour
{
    [SerializeField]
    private MobInitializable mob;

    [SerializeField]
    private Boid boid;

    private void Awake()
    {
        mob.OnMobInitialize += (_, gameState) =>
        {
            boid.AttachToManager(gameState.Services.MobManager.BoidManager);
            Destroy(this);
        };
    }
}
