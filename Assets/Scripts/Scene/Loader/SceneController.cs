using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public interface ISceneController
{
    bool IsSceneLoadInProgress { get; }
    void LoadScene(SceneIdentifier scene, System.Action<ISceneUnloader> scope);
}

[RequireComponent(typeof(SceneTransition))]
public class SceneController : MonoBehaviour, ISceneController
{
    private static SceneController instance;
    public static ISceneController Instance => instance;

    [SerializeField]
    private Transform temporaryStorage;

    private SceneTransition transition;
    private bool sceneLoadInProgress = false;

    private Scene PersistantScene => gameObject.scene;

    public bool IsSceneLoadInProgress => sceneLoadInProgress;

    private void Awake()
    {
        Assert.IsNull(instance);
        instance = this;

        transition = GetComponent<SceneTransition>();
    }

    public void LoadScene(SceneIdentifier scene, System.Action<ISceneUnloader> scope)
    {
        Assert.IsFalse(sceneLoadInProgress);
        if (sceneLoadInProgress)
            return;

        sceneLoadInProgress = true;
        StartCoroutine(LoadSceneRoutine(scene, scope));
    }

    public IEnumerator LoadSceneRoutine(SceneIdentifier scene, System.Action<ISceneUnloader> scope)
    {
        yield return 0;

        Debug.Log($"Scene change requested: targetScene={scene.name}");

        var unloader = new SceneUnloader(PersistantScene, temporaryStorage);

        // 1. UI animation

            // TODO

        // 2. Scene de-init

        scope.Invoke(unloader);

        // 3. Scene unloading

        var currentScene = SceneManager.GetActiveScene();
        Debug.Log($"Unloading current scene: {currentScene.name}");
        var unloadOperation = SceneManager.UnloadSceneAsync(currentScene);
        if (unloadOperation != null)
            yield return new WaitUntil(() => unloadOperation.isDone);

        Debug.Log($"Unloading current scene complete");

        yield return 0;

        // 4. Scene loading

        Debug.Log($"Loading in scene: {scene.name}");

        var loadOperation = SceneManager.LoadSceneAsync(scene.name, LoadSceneMode.Additive);
        yield return new WaitUntil(() => loadOperation.isDone);
        var newScene = SceneManager.GetSceneByName(scene.name);
        SceneManager.SetActiveScene(newScene);
        Assert.IsTrue(newScene.isLoaded);

        Debug.Log($"Finished loading scene");

        // 5. Scene init

        foreach (var gameObject in newScene.GetRootGameObjects())
        {
            if (gameObject.TryGetComponent<SceneInitializer>(out var initializer))
            {
                initializer.InitializeScene(unloader.UnloadInScene(newScene), CompleteSceneLoad);
                yield break;
            }
        }

        CompleteSceneLoad();
    }

    private void CompleteSceneLoad()
    {
        // Cleanup temporary storage
        Assert.IsTrue(temporaryStorage.childCount == 0);
        for (int i = 0; i < temporaryStorage.childCount; i++)
        {
            Destroy(temporaryStorage.GetChild(i).gameObject);
        }

        // 6. UI animation

        // TODO

        // 7. Complete

        Debug.Log("Scene initialization complete");
        sceneLoadInProgress = false;
    }
}
