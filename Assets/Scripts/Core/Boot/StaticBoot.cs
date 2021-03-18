using UnityEngine;
using UnityEngine.SceneManagement;

public static class StaticBoot
{
    [RuntimeInitializeOnLoadMethod]
    private static void Init()
    {

#if UNITY_EDITOR
        Debug.Log("Limiting target frame rate to 60...");
        Application.targetFrameRate = 60;
#endif

        SceneManager.LoadScene(SceneIdentifier.PERSISTANT, LoadSceneMode.Additive);
    }
}
