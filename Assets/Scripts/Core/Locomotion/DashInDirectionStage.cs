using UnityEngine;
using System.Collections;

public class DashInDirectionStage : SequenceStage
{
    protected override ITweenActionProvider Provider { get; }

    public DashInDirectionStage(DashActor actor, Vector2 direction)
    {
        Provider = new TweenBuilder()
            .ThenDo(() => actor.DashInDirection(direction))
            .ThenWaitFrame()
            .ThenWaitUntil(() => !actor.IsDashing);
    }
}
