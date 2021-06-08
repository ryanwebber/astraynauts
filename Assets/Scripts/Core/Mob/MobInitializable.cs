using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Mob))]
public class MobInitializable : MonoBehaviour
{
    public Event<Mob, GameState> OnMobInitialize;

    public Mob Initialize(GameState gameState)
    {
        var mob = GetComponent<Mob>();

        // Initialize the common set of mob components
        TryInitializeComponent<Boid>(boid =>
            boid.AttachToManager(gameState.Services.MobManager.BoidManager));

        TryInitializeComponent<NavigationTopologyInfluencer>(influencer =>
            influencer.Topology = gameState.Services.MobManager.NavigationService.NavigationTopology);

        // Initialize anything else via the events
        OnMobInitialize?.Invoke(mob, gameState);

        return mob;
    }

    private void TryInitializeComponent<T>(System.Action<T> initializer)
    {
        if (TryGetComponent<T>(out var c))
            initializer?.Invoke(c);
    }

    private void Start()
    {
        Destroy(this);
    }
}
