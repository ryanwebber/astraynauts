using UnityEngine;
using System.Collections;

public abstract class SequenceStage : ITweenActionProvider
{
    protected abstract ITweenActionProvider Provider { get; }
    public ITweenAction Action => Provider.Action;
}
