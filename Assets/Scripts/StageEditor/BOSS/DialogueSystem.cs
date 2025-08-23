using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System;

public class DialogueSystem : MonoBehaviour
{
    public PlayerController player;
    [Header("Components")]
    public SpriteRenderer leftPortrait;
    public SpriteRenderer rightPortrait;
    public SpriteRenderer dialogueBox;
    public TextMeshPro dialogueText;
    public TextMeshPro leftNameText;
    public TextMeshPro rightNameText;

    private bool zPressedLastFrame = false;
    private bool ctrlPressed => Input.GetKey(KeyCode.LeftControl);

    private bool leftPortraitShown = false;
    private bool rightPortraitShown = false;

    private void Awake()
    {
        SetSorting(leftNameText, "EBullet", 100);  // 示例
        SetSorting(rightNameText, "EBullet", 100);
        SetSorting(dialogueText, "EBullet", 100);  // 比名字稍低   
    }
    private void OnEnable()
    {
        ResetVisuals();

    }
    private void ResetVisuals()
    {
        
        leftPortrait.DOKill();
        rightPortrait.DOKill();
        dialogueBox.DOKill();
        dialogueText.DOKill();

        leftPortrait.color = Color.clear;
        rightPortrait.color = Color.clear;
        leftPortrait.gameObject.SetActive(false);
        rightPortrait.gameObject.SetActive(false);

        dialogueBox.color = Color.clear;
        dialogueText.color = Color.clear;
        dialogueBox.gameObject.SetActive(false);
        dialogueText.gameObject.SetActive(false);

        if (leftNameText != null)
        {
            leftNameText.DOKill();
            leftNameText.text = "";
            leftNameText.color = Color.clear;
            leftNameText.gameObject.SetActive(false);
        }
        if (rightNameText != null)
        {
            rightNameText.DOKill();
            rightNameText.text = "";
            rightNameText.color = Color.clear;
            rightNameText.gameObject.SetActive(false);
        }

        leftPortraitShown = false;
        rightPortraitShown = false;
        zPressedLastFrame = false;
    }

    public IEnumerator PlayDialogue(List<DialogueLine> lines)
    {
        player.InConversation = true;
        dialogueBox.gameObject.SetActive(true);
        dialogueText.gameObject.SetActive(true);

        dialogueBox.color = new Color(1, 1, 1, 0);
        dialogueBox.DOFade(1f, 0.5f);

        dialogueText.color = new Color(1, 1, 1, 0);
        dialogueText.DOFade(1f, 0.5f);

        foreach (var line in lines)
        {
            if (line.BGM != null) 
            {
                GeneralAudioPool.Instance.StopAll();
                GeneralAudioPool.Instance.Play(line.BGM,1f,true);
                GameObject obj = Instantiate(line.BGMName,new Vector2(0.164f, 2.59f),transform.rotation);
            }
            dialogueText.text = line.content;

            Sprite sprite = line.portraitSprite;
            SpriteRenderer speaker = line.isLeftSide ? leftPortrait : rightPortrait;
            SpriteRenderer other = line.isLeftSide ? rightPortrait : leftPortrait;

            bool isLeft = line.isLeftSide;
            bool isFirstTimeSpeaker = (isLeft && !leftPortraitShown) || (!isLeft && !rightPortraitShown);
            
            // 标记为已显示
            if (isLeft) leftPortraitShown = true;
            else rightPortraitShown = true;

            TextMeshPro currentNameText = line.isLeftSide ? leftNameText : rightNameText;

            if (currentNameText != null && !string.IsNullOrEmpty(line.speakerName))
            {
                currentNameText.gameObject.SetActive(true);
                currentNameText.DOKill(); // 清理旧动画
                currentNameText.text = line.speakerName;
                currentNameText.color = line.nameColor;
                Sequence nameSeq = DOTween.Sequence();
                nameSeq.Append(currentNameText.DOFade(1f, 0.5f));
                nameSeq.AppendInterval(3f);
                nameSeq.Append(currentNameText.DOFade(0f, 0.5f));
            }
            // 初始化并激活当前立绘
            if (isFirstTimeSpeaker)
            {
                // 初次出现：做淡入 + scale 动画
                if (!speaker.gameObject.activeSelf)
                    speaker.gameObject.SetActive(true);
                speaker.color = new Color(1, 1, 1, 0);
                speaker.sprite = sprite;
                speaker.gameObject.SetActive(true);
                speaker.DOFade(1f, 0.2f);
            }
            else
            {
                // 后续出现：直接切换 sprite、保持显示状态
                speaker.DOKill();
                speaker.DOFade(1f, 0.2f);
                speaker.sprite = sprite;
            }

            // 另一侧弱化显示
            bool otherShown = isLeft ? rightPortraitShown : leftPortraitShown;
            if (otherShown)
            {
                if (!other.gameObject.activeSelf) other.gameObject.SetActive(true);
                other.DOKill();
                other.DOFade(0.4f, 0.2f);
            }
            if (line.cannotSkip)
            {
                yield return new WaitForSeconds(line.lineTimeInterval);
            }
            else 
            {
                yield return WaitForInputOrTimeout(line.lineTimeInterval);
            }
            
        }
        player.InConversation = false;
        // 全部淡出
        leftPortrait.DOFade(0f, 0.3f);
        rightPortrait.DOFade(0f, 0.3f);
        dialogueBox.DOFade(0f, 0.3f);
        dialogueText.DOFade(0f, 0.3f);
        if (leftNameText != null) leftNameText.DOFade(0f, 0.3f);
        if (rightNameText != null) rightNameText.DOFade(0f, 0.3f);

        yield return new WaitForSeconds(0.3f);
        ResetVisuals();
    }

    private IEnumerator WaitForInputOrTimeout(float timeout)
    {
        float t = 0f;
        // 避免一进来就误判按住Z为一次点击（非常重要）
        zPressedLastFrame = true;
        yield return null; // 跳过1帧
        zPressedLastFrame = Input.GetKey(KeyCode.Z);

        while (t < timeout)
        {
            // Ctrl 快进：直接跳
            if (ctrlPressed)
                yield break;

            bool zNow = Input.GetKey(KeyCode.Z);

            // Z 被“重新按下”了
            if (zNow && !zPressedLastFrame)
            {
                zPressedLastFrame = true;
                yield break;
            }

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