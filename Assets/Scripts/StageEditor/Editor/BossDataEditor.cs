using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BossData))]
public class BossDataEditor : Editor
{
    SerializedProperty bossPrefab;
    SerializedProperty spawnPosition;
    SerializedProperty entryTargetPosition;
    SerializedProperty entryDuration;
    SerializedProperty spellBG;
    SerializedProperty events;

    void OnEnable()
    {
        bossPrefab = serializedObject.FindProperty("bossPrefab");
        spawnPosition = serializedObject.FindProperty("spawnPosition");
        entryTargetPosition = serializedObject.FindProperty("entryTargetPosition");
        entryDuration = serializedObject.FindProperty("entryDuration");
        spellBG = serializedObject.FindProperty("spellBG");
        events = serializedObject.FindProperty("events");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(bossPrefab);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Spawn Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(spawnPosition);
        EditorGUILayout.PropertyField(entryTargetPosition);
        EditorGUILayout.PropertyField(entryDuration);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Visuals", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(spellBG);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Action Events", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(events, true);

        serializedObject.ApplyModifiedProperties();
    }
}
