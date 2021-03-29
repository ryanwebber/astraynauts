using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public interface IDebugUI
{
    void Set(string key, string value);
    void Unset(string key);
}

public class DebugUI : MonoBehaviour, IDebugUI
{
    private static DebugUI instance;
    public static IDebugUI Instance => instance;

    [SerializeField]
    private TextMeshProUGUI uiText;

    private SortedList<string, string> data;

    private void Awake()
    {
        instance = this;
        data = new SortedList<string, string>();
    }

    private void Start()
    {
        StartCoroutine(LogSystemStats());
        SceneManager.activeSceneChanged += (_, scene) => {
            Set("system.scene", scene.name);
        };

        var currentScene = SceneManager.GetActiveScene();
        Set("system.scene", currentScene.name);
    }

    private IEnumerator LogSystemStats()
    {
        while (true)
        {
            float time = Time.unscaledDeltaTime;
            if (time == 0)
                Set("system.fps", "∞");
            else
                Set("system.fps", Mathf.FloorToInt(1f / time).ToString());

            yield return new WaitForSeconds(0.25f);
        }
    }

    public void Set(string key, string value)
    {
        if (data.ContainsKey(key))
        {
            if (data[key] == value)
                return;

            data[key] = value;
        }
        else
        {
            data.Add(key, value);
        }

        UpdateText();
    }

    public void Unset(string key)
    {
        data.Remove(key);
        UpdateText();
    }

    private void UpdateText()
    {
        string text = "";
        foreach (var entry in data)
        {
            text += $"{entry.Key}: {entry.Value}";
            text += "\n";
        }

        uiText.text = text.TrimEnd();
    }
}
