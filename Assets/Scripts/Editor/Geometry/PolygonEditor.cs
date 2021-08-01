using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(Polygon))]
public class PolygonEditor : Editor
{
    private Polygon polygon;

    private void OnEnable()
    {
        polygon = target as Polygon;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUI.BeginChangeCheck();
        if (GUILayout.Button("Auto Calculate"))
        {
            if (TryExtract<BoxCollider2D>(out var boxCollider))
            {
                Undo.RegisterCompleteObjectUndo(polygon, "Auto Compute Polygon Bounds");

                var bounds = boxCollider.bounds;
                bounds.Expand(-0.05f);
                polygon.Points.Clear();
                polygon.Points.Add(new Vector2(bounds.min.x, bounds.max.y));
                polygon.Points.Add(new Vector2(bounds.max.x, bounds.max.y));
                polygon.Points.Add(new Vector2(bounds.max.x, bounds.min.y));
                polygon.Points.Add(new Vector2(bounds.min.x, bounds.min.y));
                EditorUtility.SetDirty(polygon);
            }
        }
    }

    private void OnSceneGUI()
    {
        Handles.color = Color.magenta;
        for (int i = 0; i < polygon.Points.Count; i++)
        {
            EditorGUI.BeginChangeCheck();
            var newPosition = Handles.PositionHandle(polygon.Points[i], Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(polygon, "Edit Polygon");
                polygon.Points[i] = newPosition;
            }

            if (i < polygon.Points.Count - 1)
                Handles.DrawLine(polygon.Points[i], polygon.Points[i + 1]);
        }

        if (polygon.Points.Count > 2)
            Handles.DrawLine(polygon.Points[0], polygon.Points[polygon.Points.Count - 1]);
    }

    private bool TryExtract<T>(out T component) where T: Component
    {
        if (polygon.TryGetComponent(out component))
            return true;

        component = polygon.GetComponentInChildren<T>();
        return component != null;
    }
}
