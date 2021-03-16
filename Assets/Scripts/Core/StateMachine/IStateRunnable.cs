using System;

public interface IStateRunnable : IState
{
    void OnEnter();
    void OnExit();
}
