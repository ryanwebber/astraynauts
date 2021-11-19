using UnityEngine;
using System.Collections;

public class SynchronizationService : MonoBehaviour
{
    [SerializeField, Min(0f)]
    private float tickDuration;

    [SerializeField]
    private GameState gameState;

    public Synchronizer Clock { get; private set; }

    private void Awake()
    {
        Clock = new Synchronizer();
        gameState.OnGameStateInitializationBegin += () =>
        {
            StartCoroutine(Synchronize());
        };
    }

    private IEnumerator Synchronize()
    {
        while (true)
        {
            yield return new WaitForSeconds(tickDuration);
            Clock.OnTick?.Invoke();
        }
    }
}
