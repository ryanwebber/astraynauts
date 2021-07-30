using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class CoroutineState : State
{
    private MonoBehaviour monoBehaviour;
    private Coroutine current;

    public CoroutineState(MonoBehaviour monoBehaviour)
    {
        this.monoBehaviour = monoBehaviour;
    }

    protected abstract IEnumerator GetCoroutine();
    protected virtual void OnExit () { }

    public sealed override void OnEnter(IStateMachine sm)
    {
        this.current = monoBehaviour.StartCoroutine(GetCoroutine());
    }

    public sealed override void OnExit(IStateMachine sm)
    {
        if (current != null)
        {
            monoBehaviour.StopCoroutine(current);
            OnExit();
            current = null;
        }
    }

    public sealed override void OnUpdate(IStateMachine sm)
    {
        // Noop
    }
}
