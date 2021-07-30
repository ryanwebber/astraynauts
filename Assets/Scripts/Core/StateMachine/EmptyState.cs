using System;

public class EmptyState : State
{
    private string _name;
    public override string Name => _name;

    public EmptyState(string name)
    {
        this._name = name;
    }
}
