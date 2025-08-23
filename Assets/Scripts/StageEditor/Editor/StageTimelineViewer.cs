using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class StageTimelineViewer : EditorWindow
{
    StageTimeline timeline;
    float totalTime = 300f; // 总时长（你可以从数据中自动算）
    float currentTime = 0f;
    Vector2 scroll;

    [MenuItem("Tools/Stage Timeline Viewer")]
    static void Open()
    {
        GetWindow<StageTimelineViewer>("Stage Timeline");
    }

    void OnGUI()
    {
        timeline = (StageTimeline)EditorGUILayout.ObjectField("Stage Timeline", timeline, typeof(StageTimeline), true);
        if (timeline == null || timeline.stageData == null) return;

        totalTime = Mathf.Max(totalTime, GetStageDuration());
        currentTime = EditorGUILayout.Slider("Current Time", currentTime, 0, totalTime);

        if (GUILayout.Button("Apply to Timeline"))
        {
            timeline.SetTime(currentTime);
        }

        DrawTimeline();
        GUILayout.Space(10);
        GUILayout.Label("Event Operations", EditorStyles.boldLabel);

        if (GUILayout.Button("Add Enemy Event at Current Time"))
        {
            var newEvent = new EnemySpawnEvent
            {
                time = currentTime,
                enemyPrefab = null,
                spawnType = EnemySpawnEvent.SpawnType.Instant,
                spawnPosition = Vector2.zero,
                tragetSpeed = new Vector2 (1f,1f)
            };

            Undo.RecordObject(timeline.stageData, "Add Enemy Event");
            timeline.stageData.enemyEvents.Add(newEvent);
            timeline.stageData.enemyEvents.Sort((a, b) => a.time.CompareTo(b.time));
            EditorUtility.SetDirty(timeline.stageData);
        }

        if (GUILayout.Button("Add Background Event at Current Time"))
        {
            var newEvent = new BackgroundEvent
            {
                time = currentTime,
                backgroundSprite = null,
                enableScroll = false,
                scrollSpeed = Vector2.zero,
                enableFade = false,
                fadeToAlpha = 1f,
                fadeDuration = 1f,
                enableRotation = false,
                rotateToAngle = 0f,
                rotateDuration = 0f,
                toggleFog = false
            };

            Undo.RecordObject(timeline.stageData, "Add Background Event");
            timeline.stageData.backgroundEvents.Add(newEvent);
            timeline.stageData.backgroundEvents.Sort((a, b) => a.time.CompareTo(b.time));
            EditorUtility.SetDirty(timeline.stageData);
        }
    }

    float GetStageDuration()
    {
        var events = timeline.stageData.enemyEvents;
        return events.Count > 0 ? events[^1].time + 5f : 60f;
    }

    void DrawTimeline()
    {
        float usableWidth = Mathf.Max(position.width - 20, 100); // 最小 100 防止溢出
        Rect rect = GUILayoutUtility.GetRect(usableWidth, 60);
        EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));

        float lineY = rect.y;

        // --- Enemy Events (Red, stacked vertically if overlap)
        Dictionary<float, int> enemyTimeCounts = new Dictionary<float, int>();

        foreach (var e in timeline.stageData.enemyEvents)
        {
            float t = Mathf.Clamp01(e.time / totalTime);
            float x = rect.x + rect.width * t;

            if (!enemyTimeCounts.ContainsKey(e.time))
                enemyTimeCounts[e.time] = 0;
            int offsetIndex = enemyTimeCounts[e.time]++;
            float yOffset = lineY + 10 + offsetIndex * 6;

            EditorGUI.DrawRect(new Rect(x - 3, yOffset, 6, 6), Color.red);
        }

        // --- Background Events (Cyan, single layer)
        foreach (var e in timeline.stageData.backgroundEvents)
        {
            float t = Mathf.Clamp01(e.time / totalTime);
            float x = rect.x + rect.width * t;
            EditorGUI.DrawRect(new Rect(x - 3, lineY + 26, 6, 6), Color.cyan);
        }

        // --- Audio Events (Yellow, single layer)
        foreach (var e in timeline.stageData.audioEvents)
        {
            float t = Mathf.Clamp01(e.time / totalTime);
            float x = rect.x + rect.width * t;
            EditorGUI.DrawRect(new Rect(x - 3, lineY + 40, 6, 6), Color.yellow);
        }

        // --- Current time indicator
        float currX = rect.x + rect.width * (currentTime / totalTime);
        EditorGUI.DrawRect(new Rect(currX - 1, lineY, 2, rect.height), Color.green);
    }

}
