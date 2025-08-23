using TMPro;
using DG.Tweening;
using UnityEngine;

public class SpellBackground : MonoBehaviour
{
    public SpriteRenderer sr;
    public GameObject spellAttactText;
    public AudioClip declareSE;
    void OnEnable()
    {
        GeneralAudioPool.Instance.PlayOneShot(declareSE);
        sr.color = new Color(1, 1, 1, 0);
        sr.gameObject.transform.localScale = Vector3.zero;
        Sequence sq = DOTween.Sequence();
        sq.Append(transform.DOScaleX(2, 0f));
        sq.Append(transform.DOScaleY(2.3f, 0f));
        sq.Append(sr.DOFade(1f, 1f)).OnComplete(() => GameObject.Instantiate(spellAttactText));
    }
}
