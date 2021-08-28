using UnityEngine;
using System.Collections;

[RequireComponent(typeof(HealthManager))]
public class PlayerHealthController : MonoBehaviour
{
    private int invincibilityCounter = 0;
    private int transparencyCounter = 0;

    private DashActor dashActor;
    private HealthManager healthManager;

    private void Awake()
    {
        dashActor = GetComponent<DashActor>();
        healthManager = GetComponent<HealthManager>();
    }

    private void PushTransparency()
    {
        transparencyCounter++;
        UpdateState();
    }

    private void PopTransparency()
    {
        transparencyCounter--;
        UpdateState();
    }

    private void PushInvincibility()
    {
        invincibilityCounter++;
        UpdateState();
    }

    private void PopInvincibility()
    {
        invincibilityCounter--;
        UpdateState();
    }

    private void UpdateState()
    {
        if (invincibilityCounter > 0)
            healthManager.SetState(HealthManager.Damagability.SHIELDED);
        else if (transparencyCounter > 0)
            healthManager.SetState(HealthManager.Damagability.TRANSPARENT);
        else
            healthManager.SetState(HealthManager.Damagability.VULNERABLE);
    }
}
