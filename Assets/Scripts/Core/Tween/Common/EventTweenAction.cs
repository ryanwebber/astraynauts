using System;
using System.Collections;

public class EventTweenAction: ITweenAction
{
    private Action action;

    public EventTweenAction(Action action)
    {
        this.action = action;
    }

    public IEnumerable GetEnumerable()
    {
        action();
        yield break;
    }
}
