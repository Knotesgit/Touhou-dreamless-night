using System.Collections;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

public class BombController : MonoBehaviour
{
    public PlayerController player;
    public GameObject bombEffectPrefab;
    public GameObject reimuCG;
    public BombUIManager bombUI;
    public AudioClip bombSE;
    public AudioClip growingSE;
    public Transform spwanPosition;
    public RectTransform cgSpawnPos;
    public float effectDuration = 1.5f;
    public float finalScale = 2f;

    private bool bombPlaying = false;

    public bool CanTriggerBomb()
    {
        return !bombPlaying && bombUI.GetBombCount() > 0;
    }

    public void TriggerBomb()
    {
        if (!CanTriggerBomb()) return;

        bombUI.UseBomb();
        PlayerScore.numOFBombUsed+=1;
        player.StartInvincibility(7f);
        StartCoroutine(PerformBombSequence());
    }

    IEnumerator PerformBombSequence()
    {
        bombPlaying = true;

        // 清弹
        foreach (var bullet in GameObject.FindGameObjectsWithTag("EBullet"))
        {
            bullet.SendMessage("BombDestroy", SendMessageOptions.DontRequireReceiver);
        }

        GeneralAudioPool.Instance.PlayOneShot(bombSE);

        Instantiate(reimuCG, cgSpawnPos);

        // Bomb特效
        GameObject bomb = Instantiate(bombEffectPrefab, spwanPosition.position, Quaternion.identity);
        Transform btf = bomb.transform;
        SpriteRenderer sr = bomb.GetComponent<SpriteRenderer>();
        sr.color = new Color(1, 1, 1, 0);
        btf.localScale = Vector3.zero;

        Sequence s = DOTween.Sequence();
        s.Append(sr.DOFade(1f, 0.3f));
        s.AppendInterval(1f);
        s.AppendCallback(() => GeneralAudioPool.Instance.PlayOneShot(growingSE));
        s.Join(btf.DOScale(finalScale, effectDuration));
        s.OnComplete(() =>
        {
            bomb.SendMessage("Fire");
            CameraShaker.ShakeOnce(0.05f, 1f); // 如需要
        });


        s.AppendInterval(1f);
        yield return s.WaitForCompletion();
        bombPlaying = false;
    }
}