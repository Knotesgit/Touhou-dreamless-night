using System.Collections;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    public Material scrollingMat;
    public Vector2 scrollSpeed = Vector2.zero;

    private float currentRotation = 0f;
    private Coroutine rotateCoroutine;

    void Update()
    {
        scrollingMat.SetVector("_ScrollSpeed", new Vector4(scrollSpeed.x, scrollSpeed.y, 0f, 0f));
        scrollingMat.SetFloat("_RotationAngle", currentRotation);
    }

    public void SetSpeed(Vector2 newSpeed)
    {
        scrollSpeed = newSpeed;
    }
    
    public void RotateTo(float targetAngle, float duration)
    {
        if (rotateCoroutine != null)
            StopCoroutine(rotateCoroutine);
        rotateCoroutine = StartCoroutine(RotateRoutine(targetAngle, duration));
    }

    public void SetRotationInstant(float angle)
    {
        currentRotation = angle;
    }

    public void ResetRotation()
    {
        currentRotation = 0f;
    }

    private IEnumerator RotateRoutine(float targetAngle, float duration)
    {
        float startAngle = currentRotation;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            currentRotation = Mathf.Lerp(startAngle, targetAngle, t);
            yield return null;
        }
        currentRotation = targetAngle;
        rotateCoroutine = null;
    }
}