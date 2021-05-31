using UnityEngine;
using System.Collections;

public class BoidSpawner : MonoBehaviour
{
    [SerializeField]
    private BoidServer server;

    [SerializeField]
    private Boid prefab;

    private void Start()
    {
        for (int i = 0; i < 100; i++)
        {
            var instance = Instantiate(prefab);
            instance.Server = server;
            instance.CurrentHeading = Random.insideUnitCircle.normalized;
            instance.transform.position = new Vector2(
                Random.Range(server.Bounds.xMin, server.Bounds.xMax),
                Random.Range(server.Bounds.yMin, server.Bounds.yMax)
            );
        }
    }
}
