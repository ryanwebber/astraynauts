using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InspectorRefreshable))]
public class InspectorRefreshableEditor : Editor
{
    private InspectorRefreshable refreshable;

    private void OnEnable()
    {
        refreshable = target as InspectorRefreshable;
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Refresh"))
        {
            refreshable?.OnInspectorRefresh?.Invoke();
        }
    }
}
