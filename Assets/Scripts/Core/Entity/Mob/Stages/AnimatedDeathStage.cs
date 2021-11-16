using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Stage))]
public class AnimatedDeathStage : MonoBehaviour
{
    [SerializeField]
    private GameObject root;

    private void Awake()
    {
        GetComponent<Stage>().OnStageBegin += () =>
        {
            Debug.Log("Death", this);
            Destroy(root ?? gameObject);
        };
    }
}
