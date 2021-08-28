using UnityEngine;
using System.Collections;
using System;

public class UseBehaviorStage : SequenceStage
{
    public UseBehaviorStage(ComponentBehavior behavior, SequenceStage innerStage)
    {
        Provider = new MultiStage(
            new TweenStage(new TweenBuilder().ThenDo(() => behavior.MakeCurrent())),
            innerStage,
            new TweenStage(new TweenBuilder().ThenDo(() => behavior.ExitIfCurrent()))
        );
    }

    protected override ITweenActionProvider Provider { get; }
}
