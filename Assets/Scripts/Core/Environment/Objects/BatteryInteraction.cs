using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerInteractable))]
public class BatteryInteraction : MonoBehaviour
{
    public PlayerInteractable Interactable { get; private set; }

    private void Awake()
    {
        Interactable = GetComponent<PlayerInteractable>();
        Interactable.OnPlayerInteractionStarted += player =>
        {
            Debug.Log("TODO: Should start charging", player);
        };
    }
}
