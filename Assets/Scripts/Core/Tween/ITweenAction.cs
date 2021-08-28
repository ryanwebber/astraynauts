using System;
using System.Collections;

public interface ITweenAction
{
    public IEnumerator GetYieldInstructions();
}

public interface ITweenActionProvider
{
    public ITweenAction Action { get; }
}

public class TweenActionProvider : ITweenActionProvider
{
    public ITweenAction Action { get; private set; }

    public TweenActionProvider(ITweenAction action)
    {
        Action = action;
    }
}
