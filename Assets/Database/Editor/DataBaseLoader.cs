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
        EditorGUILayout.PropertyField(serializedObject.FindProperty("jsonRaw"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("eventsData"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("optionsData"));

        // Create a new section for the "Load Data" button
        GUILayout.BeginVertical();
        GUILayout.Label("Load Data");
        if (GUILayout.Button("Load"))
        {
            _database.LoadData();
        }
        GUILayout.EndVertical();
    }

}