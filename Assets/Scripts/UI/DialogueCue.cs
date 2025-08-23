using UnityEngine;

public enum BgTransition { None, CrossFade }

[System.Serializable]
public class DialogueCue
{
    public AudioClip BGM;
    public GameObject BGMName;

    [TextArea(1, 3)]
    public string content;

    // ��ͨ�԰�
    public Sprite portraitSprite;
    public bool isLeftSide = true;

    // �԰�
    public bool isNarration = false;
    [Range(0f, 1f)] public float narrationPortraitAlpha = 0f;

    // ��������ѡ��
    public Sprite background;
    public BgTransition bgTransition = BgTransition.None; // Ĭ�ϲ���
    public float bgTransitionDuration = 0.6f;

    // ʱ��
    public float lineTimeInterval = 10f;
    public bool cannotSkip = false;
}
