using UnityEngine;
using UnityEngine.SceneManagement;

public static class StaticBoot
{
    [RuntimeInitializeOnLoadMethod]
    private static void Init()
    {

        Debug.Log($@"Booting {Application.productName}@{Application.version}
    Product Identifier: {Application.identifier}
    Platform: {Application.platform}
    Unity Version: {Application.unityVersion}
    Installer: {Application.installerName}
    Build GUID: {Application.buildGUID}
    DataPath: {Application.dataPath}
    Persistant Data Path: {Application.persistentDataPath}
    System Language: {Application.systemLanguage}
"
        );

#if UNITY_EDITOR
        Debug.Log("Limiting target frame rate to 60...");
        Application.targetFrameRate = 60;
#endif

        if (SceneManager.sceneCount > 0)
        {
            var currentScene = SceneManager.GetSceneAt(0);
            SceneManager.SetActiveScene(currentScene);
            Debug.Log($"Active scene is: {currentScene.name}");
        }    

        SceneManager.LoadScene(SceneIdentifier.PERSISTANT, LoadSceneMode.Additive);
    }
}
