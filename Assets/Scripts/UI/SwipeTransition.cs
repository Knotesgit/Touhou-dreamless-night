using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class SwipeTransition : MonoBehaviour
{
    public RectTransform swipeImage;
    public float duration = 1f;
    public Vector2 hiddenPos = new Vector2(-1920f, 0f);
    public Vector2 visiblePos = new Vector2(0f, 0f);
    
    public AudioClip swipeSE;

    private static SwipeTransition instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            swipeImage.anchoredPosition = hiddenPos;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public static void TransitionToScene(string sceneName)
    {
        if (instance == null)
        {
            Debug.LogError("SwipeTransition not found in scene!");
            return;
        }
        instance.StartCoroutine(instance.DoTransition(sceneName));
    }

    private IEnumerator DoTransition(string sceneName)
    {
        GeneralAudioPool.Instance.PlayOneShot(swipeSE,20f);
        // ��ɨ����
        swipeImage.anchoredPosition = hiddenPos;
        yield return swipeImage.DOAnchorPos(visiblePos, duration).SetEase(Ease.InOutQuad).WaitForCompletion();
        DOTween.KillAll();
        // �ȴ���������
        yield return SceneManager.LoadSceneAsync(sceneName);

        // ��ɨ�˳���������
        yield return swipeImage.DOAnchorPos(-hiddenPos, duration).SetEase(Ease.InOutQuad).WaitForCompletion();

        swipeImage.anchoredPosition = hiddenPos; // ����
    }
}
