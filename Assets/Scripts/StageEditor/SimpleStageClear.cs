using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleStageClear_InstantiateOnly : MonoBehaviour
{
    public GameObject stageClearPrefab;   // ���Կؽű����Լ�����/����
    public GameObject blackOverlayPrefab; // ���Կؽű����Լ�����1�����£�
    public string endingSceneName = "Ending";
    public float waitBeforeClear = 0.2f;  // ���Ƶ����֡�CLEAR���ļ��
    public float showClearTime = 1.0f;    // ��CLEAR��չʾʱ��
    public float fadeTime = 1.0f;         // ��Ļ�����ֵ���ʱ��
    public bool fadeOnlyLoopMusic = true; // ֻ���� loop ���� BGM
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

        // 1) ��Ļ��ס����Ļ�Լ������룩
        if (blackOverlayPrefab) Instantiate(blackOverlayPrefab);

        // 2) ȫ���ֵ��������Ļͬ����
        if (GeneralAudioPool.Instance)
            GeneralAudioPool.Instance.FadeOutAll(fadeTime, onlyLoop: fadeOnlyLoopMusic, stopAtEnd: true);

        // 3) �ȵ�����������ʵʱʱ�䣬����ʱ��Ӱ�죩
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        SceneManager.LoadScene(endingSceneName);
    }
}
