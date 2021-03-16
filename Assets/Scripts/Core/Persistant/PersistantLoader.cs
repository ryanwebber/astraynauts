using UnityEngine;
using UnityEngine.SceneManagement;

public static class PersistantLoader
{
    [RuntimeInitializeOnLoadMethod]
    private static void Init()
    {
        SceneManager.LoadScene(SceneIdentifier.PERSISTANT, LoadSceneMode.Additive);
    }
}
