using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleStageClear_InstantiateOnly : MonoBehaviour
{
    public GameObject stageClearPrefab;   // 带自控脚本，自己出现/动画
    public GameObject blackOverlayPrefab; // 带自控脚本，自己淡到1（见下）
    public string endingSceneName = "Ending";
    public float waitBeforeClear = 0.2f;  // 击破到出现“CLEAR”的间隔
    public float showClearTime = 1.0f;    // “CLEAR”展示时间
    public float fadeTime = 1.0f;         // 黑幕与音乐淡出时间
    public bool fadeOnlyLoopMusic = true; // 只淡出 loop 当作 BGM
    private PlayerController player;


    bool transitioning;

    void Awake()
    {
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        if(BossController.Instance != null)
            BossController.Instance.OnBossDefeated += OnBossDefeated;
    }
    void OnDestroy()
    {
        if (BossController.Instance != null)
            BossController.Instance.OnBossDefeated -= OnBossDefeated;
    }


    void OnBossDefeated()
    {
        if (!transitioning) StartCoroutine(Co_Run());
    }

    System.Collections.IEnumerator Co_Run()
    {
        transitioning = true;
        player.InConversation = true;
        yield return new WaitForSeconds(waitBeforeClear);
        if (stageClearPrefab) Instantiate(stageClearPrefab);

        yield return new WaitForSeconds(showClearTime);

        // 1) 黑幕盖住（黑幕自己做淡入）
        if (blackOverlayPrefab) Instantiate(blackOverlayPrefab);

        // 2) 全音乐淡出（与黑幕同步）
        if (GeneralAudioPool.Instance)
            GeneralAudioPool.Instance.FadeOutAll(fadeTime, onlyLoop: fadeOnlyLoopMusic, stopAtEnd: true);

        // 3) 等淡出结束（用实时时间，避免时缓影响）
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        SceneManager.LoadScene(endingSceneName);
    }
}
