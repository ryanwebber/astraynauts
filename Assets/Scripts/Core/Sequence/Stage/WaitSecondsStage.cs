using System;

public sealed class WaitSecondsStage : SequenceStage
{
    public float Duration { get; }

    public WaitSecondsStage(float duration)
    {
        Duration = duration;
    }

    protected override ITweenActionProvider Provider => new TweenBuilder().ThenWait(Duration);
}
