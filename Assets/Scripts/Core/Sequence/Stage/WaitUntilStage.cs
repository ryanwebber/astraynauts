using System;

public sealed class WaitUntilAction: SequenceStage
{
    public WaitUntilAction(Func<bool> predicate)
    {
        Provider = new TweenBuilder()
            .ThenWaitUntil(predicate);
    }

    public WaitUntilAction(ref Event waitFor)
    {
        var eventHasTriggered = false;
        Provider = new TweenBuilder()
            .ThenWaitUntil(() => eventHasTriggered);

        waitFor += () => eventHasTriggered = true;
    }

    protected override ITweenActionProvider Provider { get; }
}
