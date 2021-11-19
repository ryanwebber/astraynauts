using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StageController : MonoBehaviour
{
    public Event OnStageChanged;

    private Coroutine currentCoroutine;
    public Stage CurrentStage { get; private set; }

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

        if (CurrentStage != null)
        {
            if (CurrentStage.IsStageActive)
                CurrentStage.OnStageEnd?.Invoke();

            CurrentStage = null;
        }

        currentCoroutine = StartCoroutine(PlaySequenceRoutine(stages));
    }

    private IEnumerator PlaySequenceRoutine(IEnumerable<Stage> stages)
    {
        foreach (var stage in stages)
        {
            CurrentStage = stage;
            stage.OnStageBegin?.Invoke();
            OnStageChanged?.Invoke();
            yield return new WaitUntil(() => !stage.IsStageActive);
        }
    }
}
