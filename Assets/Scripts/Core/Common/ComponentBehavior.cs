using UnityEngine;
using System.Collections;

public delegate void EventBinder(ref Event ev);

public class ComponentBehavior : MonoBehaviour
{
    public Event OnEnable;
    public Event OnDisable;

    public bool IsCurrent => Multiplexer.CurrentBehavior == this;

    [SerializeField]
    public ComponentMultiplexer Multiplexer;

    public void MakeCurrent()
    {
        Multiplexer.MakeCurrent(this);
    }

    public void ExitIfCurrent()
    {
        Multiplexer.ExitIfCurrent(this);
    }

    public ComponentBehavior Bind(IActivatable activatable)
    {
        activatable.IsActive = false;
        OnEnable += () => activatable.IsActive = true;
        OnDisable += () => activatable.IsActive = false;
        return this;
    }

    public ComponentBehavior BindCoroutines(MonoBehaviour behaviour)
    {
        OnDisable += () => behaviour.StopAllCoroutines();
        return this;
    }

    public ComponentBehavior BindOnDisable(EventBinder binder)
    {
        binder?.Invoke(ref OnDisable);
        return this;
    }

    public ComponentBehavior BindOnEnable(EventBinder binder)
    {
        binder?.Invoke(ref OnEnable);
        return this;
    }

    private void OnValidate()
    {
        if (Multiplexer == null)
            Multiplexer = GetComponentInParent<ComponentMultiplexer>();
    }
}
