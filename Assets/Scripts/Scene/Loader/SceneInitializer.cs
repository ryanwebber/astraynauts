using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SceneInitializer : MonoBehaviour
{
    public Event OnSceneInitializationComplete;

    private HashSet<System.Action<ISceneLoader, System.Action>> hooks =
        new HashSet<System.Action<ISceneLoader, System.Action>>();

    public void RegisterCallback(System.Action<ISceneLoader, System.Action> callback)
    {
        hooks.Add(callback);
    }

    public void RegisterEditorCallback(System.Action callback)
    {
        if (SceneController.Instance == null || !SceneController.Instance.IsSceneLoadInProgress)
            StartCoroutine(Coroutines.Next(callback));
    }

    public void InitializeScene(ISceneLoader loader, System.Action callback)
    {
        StartCoroutine(IntializeAll(loader, callback));
    }

    private IEnumerator IntializeAll(ISceneLoader loader, System.Action callback)
    {
        var callbacksReceived = 0;
        var hooksCopy = new HashSet<System.Action<ISceneLoader, System.Action>>(hooks);
        hooks.Clear();

        System.Action receiver = () => callbacksReceived++;

        foreach (var hook in hooksCopy)
        {
            if (hook != null)
            {
                hook?.Invoke(loader, receiver);
            }
        }

        yield return new WaitUntil(() => callbacksReceived == hooksCopy.Count);

        OnSceneInitializationComplete?.Invoke();

        callback?.Invoke();
    }
}
