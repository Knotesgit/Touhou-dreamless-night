using System.Collections;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    public static CameraShaker Instance;
    private Vector3 originalPos;

    void Awake() => Instance = this;
    void Start() => originalPos = transform.localPosition;

    public static void ShakeOnce(float intensity, float duration)
    {
        Instance.StopAllCoroutines();
        Instance.StartCoroutine(Instance.DoShake(intensity, duration));
    }

    IEnumerator DoShake(float intensity, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            Vector2 offset = Random.insideUnitCircle * intensity;
            transform.localPosition = originalPos + (Vector3)offset;
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPos;
    }
}