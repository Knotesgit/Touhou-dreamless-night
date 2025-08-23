using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class DialogueDirector : MonoBehaviour
{
    [Header("Components")]
    public SpriteRenderer leftPortrait;
    public SpriteRenderer rightPortrait;
    public SpriteRenderer dialogueBox;
    public TextMeshPro dialogueText;

    [Header("Background (dual buffer)")]
    public SpriteRenderer bgA;
    public SpriteRenderer bgB;

    [Header("Timeline")]
    public List<DialogueCue> lines;       // 你要的字段名：a
    [Header("Ending")]
    public EndingDirector endingDirector;
    private bool useA = true;
    private bool zPressedLastFrame = false;
    private bool ctrlPressed => Input.GetKey(KeyCode.LeftControl);
    private bool leftPortraitShown = false;
    private bool rightPortraitShown = false;

    private void Awake()
    {
        SetSorting(dialogueText, "EBullet", 100);
    }
    private void Start()
    {
        // 进场即播
        ResetVisuals();
        if (lines != null && lines.Count > 0)
            StartCoroutine(PlayDialogue(lines));
    }

    private void ResetVisuals()
    {
        leftPortrait.DOKill(); rightPortrait.DOKill();
        dialogueBox.DOKill(); dialogueText.DOKill();

        leftPortrait.color = Color.clear;
        rightPortrait.color = Color.clear;
        leftPortrait.gameObject.SetActive(false);
        rightPortrait.gameObject.SetActive(false);

        dialogueBox.color = Color.clear;
        dialogueText.color = Color.clear;
        dialogueBox.gameObject.SetActive(false);
        dialogueText.gameObject.SetActive(false);

        leftPortraitShown = false;
        rightPortraitShown = false;
        zPressedLastFrame = false;
    }

    public IEnumerator PlayDialogue(List<DialogueCue> lines)
    {
        dialogueBox.gameObject.SetActive(true);
        dialogueText.gameObject.SetActive(true);

        dialogueBox.color = new Color(1, 1, 1, 0);
        dialogueBox.DOFade(1f, 1f);
        dialogueText.color = new Color(1, 1, 1, 0);
        dialogueText.DOFade(1f, 1f);

        foreach (var line in lines)
        {
            // 背景（可选）
            if (line.background != null)
                yield return ApplyBackground(line.background, line.bgTransition, line.bgTransitionDuration);

            // BGM（可选）
            if (line.BGM != null)
            {
                GeneralAudioPool.Instance.StopAll();
                GeneralAudioPool.Instance.Play(line.BGM, 1f, true);
                Instantiate(line.BGMName, new Vector2(4.3f, 3.6f), transform.rotation);
            }

            dialogueText.text = line.content;

            if (line.isNarration)
            {
                // 旁白：两侧统一淡到指定 Alpha
                EnsureActive(leftPortrait);
                EnsureActive(rightPortrait);
                leftPortrait.DOKill();
                leftPortrait.DOFade(line.narrationPortraitAlpha, 0.2f);
                rightPortrait.DOKill();
                rightPortrait.DOFade(line.narrationPortraitAlpha, 0.2f).OnComplete(() => 
                {
                    leftPortrait.sprite = null;
                    rightPortrait.sprite = null;
                });
                
                // 如需旁白居中：取消注释
                // dialogueText.alignment = TextAlignmentOptions.Center;
            }
            else
            {
                // dialogueText.alignment = TextAlignmentOptions.TopLeft;

                Sprite sprite = line.portraitSprite;
                SpriteRenderer speaker = line.isLeftSide ? leftPortrait : rightPortrait;
                SpriteRenderer other = line.isLeftSide ? rightPortrait : leftPortrait;

                bool isLeft = line.isLeftSide;
                bool first = (isLeft && !leftPortraitShown) || (!isLeft && !rightPortraitShown);
                if (isLeft) leftPortraitShown = true; else rightPortraitShown = true;

                EnsureActive(speaker);
                if (first)
                {
                    speaker.color = new Color(1, 1, 1, 0);
                    speaker.sprite = sprite;
                    speaker.DOFade(1f, 0.2f);
                }
                else
                {
                    speaker.DOKill();
                    speaker.sprite = sprite;
                    speaker.DOFade(1f, 0.2f);
                }

                bool otherShown = isLeft ? rightPortraitShown : leftPortraitShown;
                if (otherShown)
                {
                    EnsureActive(other);
                    other.DOKill();
                    other.DOFade(0.4f, 0.2f);
                }
            }

            if (line.cannotSkip) yield return new WaitForSeconds(line.lineTimeInterval);
            else yield return WaitForInputOrTimeout(line.lineTimeInterval);
        }

        // 结尾淡出
        leftPortrait.DOFade(0f, 0.3f);
        rightPortrait.DOFade(0f, 0.3f);
        dialogueBox.DOFade(0f, 0.3f);
        dialogueText.DOFade(0f, 0.3f);
        yield return new WaitForSeconds(0.3f);

        ResetVisuals();
        endingDirector.TriggerEnding();
    }

    private IEnumerator ApplyBackground(Sprite sprite, BgTransition t, float dur)
    {
        // 不变或没图 → 直接返回
        if (t == BgTransition.None || sprite == null)
            yield break;

        if (bgA == null || bgB == null)
            yield break;

        var cur = useA ? bgA : bgB;
        var nxt = useA ? bgB : bgA;

        // 如果当前已经是同一张图，没必要淡切
        if (cur.sprite == sprite && cur.color.a > 0.99f)
            yield break;

        // 仅 CrossFade
        if (t == BgTransition.CrossFade)
        {
            // 准备下一张
            if (!nxt.gameObject.activeSelf) nxt.gameObject.SetActive(true);
            nxt.DOKill();
            nxt.sprite = sprite;
            nxt.color = new Color(1, 1, 1, 0f);

            var fadeIn = nxt.DOFade(1f, dur);
            var fadeOut = cur.DOFade(0f, dur);
            yield return DOTween.Sequence().Join(fadeIn).Join(fadeOut).WaitForCompletion();

            useA = !useA;
        }
    }

    private void EnsureActive(SpriteRenderer r)
    {
        if (!r.gameObject.activeSelf) r.gameObject.SetActive(true);
    }

    private IEnumerator WaitForInputOrTimeout(float timeout)
    {
        float t = 0f;
        zPressedLastFrame = true;
        yield return null; // 跳过1帧，防误触
        zPressedLastFrame = Input.GetKey(KeyCode.Z);

        while (t < timeout)
        {
            if (ctrlPressed) yield break;

            bool zNow = Input.GetKey(KeyCode.Z);
            if (zNow && !zPressedLastFrame) yield break;

            zPressedLastFrame = zNow;
            t += Time.deltaTime;
            yield return null;
        }
    }

    void SetSorting(TextMeshPro tmp, string layer, int order)
    {
        if (tmp == null) return;
        var renderer = tmp.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sortingLayerName = layer;
            renderer.sortingOrder = order;
        }
    }
}
