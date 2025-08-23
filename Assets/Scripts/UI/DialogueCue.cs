using UnityEngine;

public enum BgTransition { None, CrossFade }

[System.Serializable]
public class DialogueCue
{
    public AudioClip BGM;
    public GameObject BGMName;

    [TextArea(1, 3)]
    public string content;

    // 普通对白
    public Sprite portraitSprite;
    public bool isLeftSide = true;

    // 旁白
    public bool isNarration = false;
    [Range(0f, 1f)] public float narrationPortraitAlpha = 0f;

    // 背景（可选）
    public Sprite background;
    public BgTransition bgTransition = BgTransition.None; // 默认不变
    public float bgTransitionDuration = 0.6f;

    // 时间
    public float lineTimeInterval = 10f;
    public bool cannotSkip = false;
}
