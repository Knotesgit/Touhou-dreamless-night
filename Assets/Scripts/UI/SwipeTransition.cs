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
        // 横扫进入
        swipeImage.anchoredPosition = hiddenPos;
        yield return swipeImage.DOAnchorPos(visiblePos, duration).SetEase(Ease.InOutQuad).WaitForCompletion();
        DOTween.KillAll();
        // 等待场景加载
        yield return SceneManager.LoadSceneAsync(sceneName);

        // 横扫退出（反方向）
        yield return swipeImage.DOAnchorPos(-hiddenPos, duration).SetEase(Ease.InOutQuad).WaitForCompletion();

        swipeImage.anchoredPosition = hiddenPos; // 重置
    }
}
