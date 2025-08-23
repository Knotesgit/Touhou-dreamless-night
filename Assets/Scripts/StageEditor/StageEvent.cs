using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// StageEvent.cs
public abstract class StageEvent
{
    public float time;
    public abstract void Execute(StageTimeline timeline);
}

public class EnemySpawnStageEvent : StageEvent
{
    public EnemySpawnEvent data;
    public override void Execute(StageTimeline timeline)
    {
        timeline.SpawnEnemy(data);
    }
}

public class BackgroundStageEvent : StageEvent
{
    public BackgroundEvent data;
    public override void Execute(StageTimeline timeline)
    {
        timeline.ApplyBackgroundEvent(data);
    }
}

public class AudioStageEvent : StageEvent
{
    public AudioEvent data;
    public override void Execute(StageTimeline timeline)
    {
        timeline.PlayAudio(data);
    }
}
public class DisplayStageEvent : StageEvent
{
    public DisplayEvent data;
    public override void Execute(StageTimeline timeline)
    {
        timeline.DisplayObject(data);
    }
}

public class BossStageEvent : StageEvent
{
    public BossEvent data;
    public override void Execute(StageTimeline timeline)
    {
        timeline.DeployBoss(data);
    }
}

