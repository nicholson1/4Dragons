using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Database))]
public class DatabaseLoader : Editor
{
    private Database _database;

    public override void OnInspectorGUI()
    {
        _database = (Database)target;
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("speadsheetID"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("credentials"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("eventsRaw"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("optionsRaw"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("outcomesRaw"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("eventsTab"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("optionsTab"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("outcomesTab"));

        GUILayout.BeginVertical();
        GUILayout.Label("Load Tabs");
        if (GUILayout.Button("Load"))
        {
            _database.LoadTabs();
        }
        GUILayout.EndVertical();
    }

}