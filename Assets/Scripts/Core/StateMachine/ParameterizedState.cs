using System;

public class ParameterizedState : IStateRunnable
{
    private string name;
    private Action onEnter;
    private Action onExit;
    private Action onUpdate;

    public string Name => name;

    public ParameterizedState(string name, Action onEnter, Action onExit, Action onUpdate)
    {
        this.name = name;
        this.onEnter = onEnter;
        this.onExit = onExit;
        this.onUpdate = onUpdate;
    }

    public void OnEnter()
    {
        onEnter?.Invoke();
    }

    public void OnExit()
    {
        onExit?.Invoke();
    }

    public void OnUpdate()
    {
        onUpdate?.Invoke();
    }
}