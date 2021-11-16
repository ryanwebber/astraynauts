using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Stage))]
public class SkipStage : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Stage>().OnStageBegin += () =>
        {
            GetComponent<Stage>().OnStageEnd?.Invoke();
        };
    }
}
