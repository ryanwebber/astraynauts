using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class DoorController : MonoBehaviour
{
    public struct State
    {
        public bool isOpen;
    }

    public Event<State> OnDoorStateChanged;

    [SerializeField]
    private string animationProperty;

    public bool IsDoorOpen => animator.GetBool(animationProperty);
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetDoorOpen(bool isOpen)
    {
        if (isOpen != IsDoorOpen)
        {
            animator.SetBool(animationProperty, isOpen);
            StartCoroutine(Coroutines.After(0.05f, () =>
            {
                if (IsDoorOpen == isOpen)
                    OnDoorStateChanged?.Invoke(new State { isOpen = isOpen });
            }));
        }
    }
}
