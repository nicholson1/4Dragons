using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DatabaseLoader))]
public class DatabaseLoaderEditor : Editor
{
    private DatabaseLoader _databaseLoader;

    public override void OnInspectorGUI()
    {
        _databaseLoader = (DatabaseLoader)target;
        serializedObject.Update();

        GUILayout.BeginVertical();
        GUILayout.Label("Load Database Tabs");
        if (GUILayout.Button("Load Database Tabs"))
        {
            _databaseLoader.LoadTabs();
        }
        GUILayout.EndVertical();
    }
}