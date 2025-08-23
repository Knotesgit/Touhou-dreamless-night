using UnityEngine;
using UnityEngine.UI;

public class RTBootstrapper : MonoBehaviour
{
    public Camera gameCamera;     // 你的主相机
    public RawImage blitTarget;   // 显示 RT 的 RawImage

    public RenderTexture template;   // 指到 InternalRT_1280x960（可空）

    RenderTexture runtimeRT;

    void OnEnable()
    {
        // 释放旧的，避免泄漏
        if (runtimeRT != null)
        {
            gameCamera.targetTexture = null;
            blitTarget.texture = null;
            runtimeRT.Release();
            Destroy(runtimeRT);
            runtimeRT = null;
        }

        // 用模板或手动描述符创建新的
        RenderTextureDescriptor desc = template
            ? template.descriptor
            : new RenderTextureDescriptor(1280, 960, RenderTextureFormat.ARGB32, 24);

        desc.msaaSamples = 1;
        desc.useMipMap = false;
        desc.autoGenerateMips = false;

        runtimeRT = new RenderTexture(desc) { name = "InternalRT_Runtime" };
        runtimeRT.Create();

        // 同帧切换相机输出与 RawImage 贴图
        gameCamera.targetTexture = runtimeRT;
        if (blitTarget != null) blitTarget.texture = runtimeRT;
    }

    void OnDisable()
    {
        if (runtimeRT != null)
        {
            gameCamera.targetTexture = null;
            if (blitTarget != null) blitTarget.texture = null;
            runtimeRT.Release();
            Destroy(runtimeRT);
            runtimeRT = null;
        }
    }
}
