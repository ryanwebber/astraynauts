using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MobLifecycle))]
[RequireComponent(typeof(SwarmMovementController))]
public class CrawlerMob : MonoBehaviour
{
    private void Awake()
    {
        var lifecycle = GetComponent<MobLifecycle>();
        var controller = GetComponent<SwarmMovementController>();

        // Start inactive
        controller.IsActive = false;

        lifecycle.OnBeginMobControl += () => controller.IsActive = true;
        lifecycle.OnEndMobControl += () => controller.IsActive = false;
    }
}
