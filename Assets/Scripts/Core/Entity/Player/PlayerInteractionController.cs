using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Player))]
public class PlayerInteractionController : MonoBehaviour
{
    public Event<PlayerInteractable> OnCurrentInteractableChanged;

    public Event OnInteractionInputBegin;
    public Event OnInteractionInputEnd;

    [SerializeField]
    private ActionReceiver actionReceiver;

    private Player player;
    private PlayerInteractable currentInteractable;

    private ActionReceiver.NonAllocRequest reusableRequest;

    private bool isLocked = true;
    public bool IsInteractionLocked {
        get => isLocked;
        set
        {
            if (value && IsBusy)
            {
                Debug.Log("Interaction controller is cancelling current interaction to become locked", this);
                EndCurrentInteraction();
            }

            isLocked = value;
        }
    }

    public bool IsBusy { get; private set; } = false;

    private void Awake()
    {
        player = GetComponent<Player>();
        reusableRequest = new ActionReceiver.NonAllocRequest(10);

        actionReceiver.OnActionTriggerBegin += _ => RefreshInteractions();
        actionReceiver.OnActionTriggerEnd += _ => RefreshInteractions();

        OnInteractionInputEnd += EndCurrentInteraction;
        OnInteractionInputBegin += () =>
        {
            if (!IsBusy && currentInteractable != null)
            {
                IsBusy = true;
                currentInteractable.StartInteraction(player);
            }
            else if (!IsBusy)
            {
                Debug.Log($"Nothing for player '{player.gameObject.name}' to interact with", this);
            }
        };
    }

    private void RefreshInteractions()
    {
        if (IsBusy)
            return;

        int numTriggers = actionReceiver.GetTriggers(reusableRequest);

        float minDistance = float.PositiveInfinity;
        PlayerInteractable bestInteractable = null;
        for (int i = 0; i < numTriggers; i++)
        {
            if (reusableRequest.triggers[i].TryGetComponent<PlayerInteractable>(out var interactable))
            {
                if (interactable.IsInteractable)
                {
                    float dist = Vector2.Distance(transform.position, interactable.transform.position);
                    if (dist < minDistance)
                    {
                        bestInteractable = interactable;
                        minDistance = dist;
                    }
                }
            }
        }

        if (bestInteractable != currentInteractable)
        {
            currentInteractable = bestInteractable;
            OnCurrentInteractableChanged?.Invoke(bestInteractable);
            if (bestInteractable == null)
                Debug.Log($"Player '{player.gameObject.name}' left interactable", this);
            else
                Debug.Log($"Player '{player.gameObject.name}' is now selecting '{currentInteractable.gameObject.name}'", this);
        }
    }

    private void EndCurrentInteraction()
    {
        if (IsBusy && currentInteractable != null)
        {
            currentInteractable.EndInteraction(player);
        }

        IsBusy = false;
        RefreshInteractions();
    }
}
