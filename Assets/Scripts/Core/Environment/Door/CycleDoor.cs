using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class CycleDoor : MonoBehaviour
{
    [SerializeField]
    private float cycleDuration;

    [SerializeField]
    private string animationProperty;

    private void Start()
    {
        var animator = GetComponent<Animator>();
        StartCoroutine(OpenCloseDoor(animator));
    }

    private IEnumerator OpenCloseDoor(Animator animator)
    {
        while (true)
        {
            Debug.Log("Door tick", this);
            animator.SetBool(animationProperty, !animator.GetBool(animationProperty));
            yield return new WaitForSeconds(cycleDuration);
        }
    }
}
