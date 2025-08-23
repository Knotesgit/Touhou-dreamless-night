using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StageTimeline))]
public class StageTimelineEditor : Editor
{
    private StageTimeline timeline;



    void OnEnable()
    {
        timeline = (StageTimeline)target;
    }

    void OnSceneGUI()
    {
        if (timeline == null || timeline.stageData == null) return;
        var events = timeline.stageData.enemyEvents;
        if (events == null) return;

        string filterGroup = timeline.editorFilterGroup;
        float startTime = timeline.editorFilterStartTime;
        float endTime = timeline.editorFilterEndTime;

        for (int i = 0; i < events.Count; i++)
        {
            var e = events[i];

            // 过滤逻辑
            if (!string.IsNullOrEmpty(filterGroup) && e.groupName != filterGroup)
                continue;
            if (e.time < startTime || e.time > endTime)
                continue;

            // ----------------- 可视化逻辑 -----------------
            Vector2 newSpawn = Handles.PositionHandle(e.spawnPosition, Quaternion.identity);
            if (newSpawn != e.spawnPosition)
            {
                Undo.RecordObject(timeline.stageData, "Move Spawn Position");
                e.spawnPosition = newSpawn;
                EditorUtility.SetDirty(timeline.stageData);
            }

            if (e.spawnType == EnemySpawnEvent.SpawnType.FlyIn)
            {
                Vector2 newFrom = Handles.PositionHandle(e.moveFrom, Quaternion.identity);
                if (newFrom != e.moveFrom)
                {
                    Undo.RecordObject(timeline.stageData, "Move MoveFrom");
                    e.moveFrom = newFrom;
                    EditorUtility.SetDirty(timeline.stageData);
                }

                Handles.DrawDottedLine(e.moveFrom, e.spawnPosition, 2f);
                Handles.Label((e.moveFrom + e.spawnPosition) / 2f, $"Path {i}");
            }

            Handles.Label(e.spawnPosition + Vector2.up * 0.3f, $"Enemy {i}");
        }
    }
}
