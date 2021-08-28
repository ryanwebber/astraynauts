using System;
using System.Collections.Generic;
using System.Linq;

public class MultiStage : SequenceStage
{
    protected override ITweenActionProvider Provider { get; }

    public MultiStage(IEnumerable<SequenceStage> stages)
    {
        Provider = stages.Aggregate(new TweenBuilder(), (builder, stage) => builder.Then(stage));
    }

    public MultiStage(params SequenceStage[] stages)
    {
        Provider = stages.Aggregate(new TweenBuilder(), (builder, stage) => builder.Then(stage));
    }
}
