using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SequenceController))]
public class CrawlerMob : MonoBehaviour
{
    [SerializeField]
    private List<ComponentBehavior> behaviors;

    private void Start()
    {
        GetComponent<SequenceController>().RunSequence(GetStages());
    }

    private IEnumerable<SequenceStage> GetStages()
    {
        foreach (var behavior in behaviors)
        {
            yield return new UseBehaviorStage(behavior, new WaitSecondsStage(10f));
        }
    }
}
