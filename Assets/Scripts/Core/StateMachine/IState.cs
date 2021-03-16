using System;

public interface IState
{
    string Name { get; }
    public void OnUpdate();
}
