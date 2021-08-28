using System;

public class TweenStage : SequenceStage
{
    protected override ITweenActionProvider Provider { get; }

    public TweenStage(ITweenActionProvider provider)
    {
        Provider = provider;
    }
}
