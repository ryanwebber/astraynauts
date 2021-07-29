using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

[RequireComponent(typeof(ActionTrigger))]
public class PlayerInteractable : MonoBehaviour
{
    [SerializeField]
    public bool IsInteractable = true;

    public Event<Player> OnPlayerInteractionStarted;
    public Event<Player> OnPlayerInteractionEnded;

    public void StartInteraction(Player player)
    {
        Assert.IsTrue(IsInteractable);

        if (IsInteractable)
        {
            Debug.Log($"Player ('{player.gameObject.name}' + '{gameObject.name}') interaction started", this);
            OnPlayerInteractionStarted?.Invoke(player);
        }

        IsInteractable = false;
    }

    public void EndInteraction(Player player)
    {
        if (IsInteractable)
        {
            Debug.Log($"Player ('{player.gameObject.name}' + '{gameObject.name}') interaction ended", this);
            OnPlayerInteractionEnded?.Invoke(player);
        }

        IsInteractable = true;
    }
}
