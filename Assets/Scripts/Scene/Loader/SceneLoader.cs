using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public interface ISceneLoader
{
    public bool TryGetContext<T>(out T context);
    public bool TryRecoverGameObject(SceneStoreKey key, out ILimboObject gameObject);
}

public interface ISceneUnloader
{
    public void SetContext<T>(T context);
    public SceneStoreKey StoreGameObject(GameObject gameObject);
}

public class SceneUnloader: ISceneUnloader
{
    private Scene persistantScene;
    private Transform storageParent;
    private Dictionary<Type, object> contextStore;
    private Dictionary<SceneStoreKey, GameObject> gameObjectStore;
    private Dictionary<GameObject, SceneStoreKey> reverseGameObjectStore;

    public SceneUnloader(Scene persistantScene, Transform storageParent)
    {
        Assert.IsTrue(persistantScene.isLoaded);
        this.persistantScene = persistantScene;
        this.storageParent = storageParent;
        this.contextStore = new Dictionary<Type, object>();
        this.gameObjectStore = new Dictionary<SceneStoreKey, GameObject>();
        this.reverseGameObjectStore = new Dictionary<GameObject, SceneStoreKey>();
    }

    public void SetContext<T>(T context)
    {
        contextStore.Add(typeof(T), context);
    }

    public SceneStoreKey StoreGameObject(GameObject gameObject)
    {
        if (reverseGameObjectStore.TryGetValue(gameObject, out var key))
            return key;

        Assert.IsFalse(gameObject.scene == persistantScene);

        SceneManager.MoveGameObjectToScene(gameObject, persistantScene);
        gameObject.transform.parent = storageParent;

        var newKey = SceneStoreKey.CreateUnique();
        gameObjectStore.Add(key, gameObject);
        reverseGameObjectStore.Add(gameObject, key);

        return newKey;
    }

    public ISceneLoader UnloadInScene(Scene targetScene)
    {
        return new SceneLoader(targetScene, contextStore, gameObjectStore);
    }
}

public class SceneLoader: ISceneLoader
{
    private Scene targetScene;
    private Dictionary<Type, object> contextStore;
    private Dictionary<SceneStoreKey, GameObject> gameObjectStore;

    public SceneLoader(Scene targetScene, Dictionary<Type, object> contextStore, Dictionary<SceneStoreKey, GameObject> gameObjectStore)
    {
        this.targetScene = targetScene;
        this.contextStore = contextStore;
        this.gameObjectStore = gameObjectStore;
    }

    public bool TryGetContext<T>(out T context)
    {
        if (contextStore.TryGetValue(typeof(T), out var value) && value is T instance)
        {
            context = instance;
            return true;
        }

        context = default;
        return false;
    }

    public bool TryRecoverGameObject(SceneStoreKey key, out ILimboObject gameObject)
    {
        if (gameObjectStore.TryGetValue(key, out var foundObject))
        {
            gameObject = new LimboObject
            {
                gameObject = foundObject,
                reparent = (transform) =>
                {
                    SceneManager.MoveGameObjectToScene(foundObject, targetScene);
                    foundObject.transform.parent = transform;
                }
            };

            return true;
        }

        gameObject = default;
        return false;
    }
}
