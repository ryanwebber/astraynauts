using UnityEngine;
using System.Collections;

public class CycleDoor : MonoBehaviour
{
    [SerializeField]
    private float cycleDuration;

    [SerializeField]
    private DoorController controller;

    private void Start()
    {
        StartCoroutine(OpenCloseDoor());
    }

    private IEnumerator OpenCloseDoor()
    {
        yield return new WaitForSeconds(Random.value * cycleDuration * 2);

        while (true)
        {
            controller.SetDoorOpen(!controller.IsDoorOpen);
            yield return new WaitForSeconds(cycleDuration);
        }
    }
}
