using UnityEngine;
using System.Collections;

public class NavigationInfluenceInitializable : MonoBehaviour
{
    [SerializeField]
    private MobInitializable mob;

    [SerializeField]
    private NavigationTopologyInfluencer influencer;

    private void Awake()
    {
        mob.OnMobInitialize += (_, gameState) =>
        {
            influencer.Topology = gameState.Services.MobManager.NavigationService.NavigationTopology;
            Destroy(this);
        };
    }
}
