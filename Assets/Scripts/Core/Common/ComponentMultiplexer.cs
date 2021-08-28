using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class ComponentMultiplexer : MonoBehaviour
{
    public ComponentBehavior CurrentBehavior { get; private set; }

    public Event<ComponentBehavior> OnBehaviorChanged;

    public void MakeCurrent(ComponentBehavior behavior)
    {
        if (behavior == CurrentBehavior)
            return;

        var oldBehavior = CurrentBehavior;
        CurrentBehavior = behavior;

        oldBehavior?.OnDisable?.Invoke();
        CurrentBehavior?.OnEnable?.Invoke();
        OnBehaviorChanged?.Invoke(CurrentBehavior);
    }

    public void ExitIfCurrent(ComponentBehavior behavior)
    {
        if (behavior != null && behavior == CurrentBehavior)
        {
            CurrentBehavior = null;
            behavior.OnDisable?.Invoke();
        }
    }
}
