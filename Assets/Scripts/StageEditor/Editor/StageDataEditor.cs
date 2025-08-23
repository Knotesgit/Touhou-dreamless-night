using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Security.Cryptography;

[CustomEditor(typeof(StageData))]
public class StageDataEditor : Editor
{
    private string currentGroupName = "NewGroup";
    private float shiftAmount = 0f;
    private bool clampToZero = true;

    // ½öÓÃÓÚ±à¼­Æ÷Ñ¡Ôñ×´Ì¬£¬²»Ðè³Ö¾Ã
    private Dictionary<object, bool> selectionMap = new();

    void OnEnable()
    {
        StageData data = (StageData)target;

        selectionMap.Clear();

        foreach (var e in data.enemyEvents)
            if (!selectionMap.ContainsKey(e)) selectionMap[e] = false;

        foreach (var e in data.backgroundEvents)
            if (!selectionMap.ContainsKey(e)) selectionMap[e] = false;

        foreach (var e in data.audioEvents)
            if (!selectionMap.ContainsKey(e)) selectionMap[e] = false;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Group Tools", EditorStyles.boldLabel);

        currentGroupName = EditorGUILayout.TextField("Group Name", currentGroupName);
        shiftAmount = EditorGUILayout.FloatField("Shift Time (s)", shiftAmount);
        clampToZero = EditorGUILayout.Toggle("Clamp to ¡Ý 0", clampToZero);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Apply To Selected"))
        {
            foreach (var kv in selectionMap)
            {
                if (kv.Value)
                {
                    Undo.RecordObject(target, "Set Group");
                    var field = kv.Key.GetType().GetField("groupName");
                    if (field != null)
                        field.SetValue(kv.Key, currentGroupName);
                }
            }
        }

        if (GUILayout.Button("Shift Group"))
            ShiftGroup(currentGroupName, shiftAmount);

        if (GUILayout.Button("Delete Group"))
            DeleteGroup(currentGroupName);

        if (GUILayout.Button("Copy Group"))
            CopyGroup(currentGroupName);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Enemy Events", EditorStyles.boldLabel);
        DrawEventList(((StageData)target).enemyEvents, (e) => e.time, (e, t) => e.time = t);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Background Events", EditorStyles.boldLabel);
        DrawEventList(((StageData)target).backgroundEvents, (e) => e.time, (e, t) => e.time = t);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Audio Events", EditorStyles.boldLabel);
        DrawEventList(((StageData)target).audioEvents, (e) => e.time, (e, t) => e.time = t);
    }

    void DrawEventList<T>(List<T> list, System.Func<T, float> getTime, System.Action<T, float> setTime) where T : class
    {
        foreach (var e in list)
        {
            if (!selectionMap.ContainsKey(e))
                selectionMap[e] = false;
            EditorGUILayout.BeginHorizontal();
            selectionMap[e] = EditorGUILayout.Toggle(selectionMap[e], GUILayout.Width(20));

            float newTime = EditorGUILayout.FloatField(getTime(e), GUILayout.Width(80));
            if (newTime != getTime(e))
            {
                Undo.RecordObject(target, "Change Time");
                setTime(e, newTime);
            }

            var field = e.GetType().GetField("groupName");
            if (field != null)
            {
                string oldGroup = (string)field.GetValue(e);
                string newGroup = EditorGUILayout.TextField(oldGroup);
                if (newGroup != oldGroup)
                {
                    Undo.RecordObject(target, "Change Group");
                    field.SetValue(e, newGroup);
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    void ShiftGroup(string groupName, float amount)
    {
        StageData data = (StageData)target;
        Undo.RecordObject(data, $"Shift Group {groupName}");

        foreach (var e in data.enemyEvents)
        {
            if (e.groupName == groupName)
            {
                float t = e.time + amount;
                if (clampToZero) t = Mathf.Max(0f, t);
                e.time = t;
            }
        }

        foreach (var e in data.backgroundEvents)
        {
            if (e.groupName == groupName)
            {
                float t = e.time + amount;
                if (clampToZero) t = Mathf.Max(0f, t);
                e.time = t;
            }
        }

        foreach (var e in data.audioEvents)
        {
            if (e.groupName == groupName)
            {
                float t = e.time + amount;
                if (clampToZero) t = Mathf.Max(0f, t);
                e.time = t;
            }
        }

        EditorUtility.SetDirty(data);
    }

    void DeleteGroup(string groupName)
    {
        StageData data = (StageData)target;
        Undo.RecordObject(data, $"Delete Group {groupName}");

        data.enemyEvents.RemoveAll(e => e.groupName == groupName);
        data.backgroundEvents.RemoveAll(e => e.groupName == groupName);
        data.audioEvents.RemoveAll(e => e.groupName == groupName);
    }

    void CopyGroup(string groupName)
    {
        StageData data = (StageData)target;
        Undo.RecordObject(data, $"Copy Group {groupName}");

        // ---------- Enemy ----------
        var newEnemies = new List<EnemySpawnEvent>();
        foreach (var e in data.enemyEvents)
        {
            if (e.groupName == groupName)
            {
                var clone = JsonUtility.FromJson<EnemySpawnEvent>(JsonUtility.ToJson(e));
                clone.time += 0.5f;
                clone.groupName = e.groupName;
                newEnemies.Add(clone);
            }
        }
        foreach (var clone in newEnemies)
        {
            data.enemyEvents.Add(clone);
            selectionMap[clone] = false; //×¢²áµ½Ñ¡Ôñ×´Ì¬
        }

        // ---------- Background ----------
        var newBackgrounds = new List<BackgroundEvent>();
        foreach (var e in data.backgroundEvents)
        {
            if (e.groupName == groupName)
            {
                var clone = JsonUtility.FromJson<BackgroundEvent>(JsonUtility.ToJson(e));
                clone.time += 0.5f;
                clone.groupName = e.groupName;
                newBackgrounds.Add(clone);
            }
        }
        foreach (var clone in newBackgrounds)
        {
            data.backgroundEvents.Add(clone);
            selectionMap[clone] = false; //×¢²á
        }

        // ---------- Audio ----------
        var newAudios = new List<AudioEvent>();
        foreach (var e in data.audioEvents)
        {
            if (e.groupName == groupName)
            {
                var clone = JsonUtility.FromJson<AudioEvent>(JsonUtility.ToJson(e));
                clone.time += 0.5f;
                clone.groupName = e.groupName;
                newAudios.Add(clone);
            }
        }
        foreach (var clone in newAudios)
        {
            data.audioEvents.Add(clone);
            selectionMap[clone] = false; //×¢²á
        }

        EditorUtility.SetDirty(data);
    }
}