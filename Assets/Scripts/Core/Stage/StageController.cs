using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StageController : MonoBehaviour
{
    private Coroutine currentCoroutine;
    private Stage currentStage;

    public void PlayStage(Stage stage)
    {
        PlaySequence(new Stage[] { stage });
    }

    public void PlaySequence(IReadOnlyList<Stage> stages)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        if (currentStage != null)
        {
            if (currentStage.IsStageActive)
                currentStage.OnStageEnd?.Invoke();

            currentStage = null;
        }

        currentCoroutine = StartCoroutine(PlaySequenceRoutine(stages));
    }

    private IEnumerator PlaySequenceRoutine(IEnumerable<Stage> stages)
    {
        foreach (var stage in stages)
        {
            currentStage = stage;
            stage.OnStageBegin?.Invoke();
            yield return new WaitUntil(() => !stage.IsStageActive);
        }
    }
}
