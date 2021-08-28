using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SequenceController : MonoBehaviour
{
    public void RunSequence(IEnumerable<SequenceStage> stages)
    {
        var multiStage = new MultiStage(stages);
        new TweenBuilder().Then(multiStage).Start(this);
    }
}
