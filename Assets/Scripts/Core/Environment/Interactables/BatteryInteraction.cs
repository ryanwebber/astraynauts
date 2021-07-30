using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerInteractable))]
public class BatteryInteraction : MonoBehaviour
{
    public PlayerInteractable Interactable { get; private set; }

    private void Awake()
    {
        Interactable = GetComponent<PlayerInteractable>();
        Interactable.OnPlayerInteractionStarted += StartCharging;
    }

    private void StartCharging(Player player)
    {
        player.State.TryStartCharging(this);
    }
}
