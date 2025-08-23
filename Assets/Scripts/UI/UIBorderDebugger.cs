using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class UIBorderDebugger : MonoBehaviour
{
    public Camera targetCamera;
    public Color color = Color.green;

    void OnDrawGizmos()
    {
        if (!targetCamera || !targetCamera.orthographic) return;

        float height = targetCamera.orthographicSize * 2;
        float width = height * targetCamera.aspect;

        Vector3 center = targetCamera.transform.position + Vector3.forward * (targetCamera.nearClipPlane + 0.1f);
        Gizmos.color = color;
        Gizmos.DrawWireCube(center, new Vector3(width, height, 0));
    }
}