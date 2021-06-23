using System;

public interface IOperation
{
    void Perform();
}

public struct LambdaOperation : IOperation
{
    private System.Action action;

    public LambdaOperation(Action action)
    {
        this.action = action;
    }

    public void Perform() => action?.Invoke();
}

public struct NoopOperation : IOperation
{
    public void Perform()
    {
    }
}
