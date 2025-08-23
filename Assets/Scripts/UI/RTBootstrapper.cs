using UnityEngine;
using UnityEngine.UI;

public class RTBootstrapper : MonoBehaviour
{
    public Camera gameCamera;     // ��������
    public RawImage blitTarget;   // ��ʾ RT �� RawImage

    public RenderTexture template;   // ָ�� InternalRT_1280x960���ɿգ�

    RenderTexture runtimeRT;

    void OnEnable()
    {
        // �ͷžɵģ�����й©
        if (runtimeRT != null)
        {
            gameCamera.targetTexture = null;
            blitTarget.texture = null;
            runtimeRT.Release();
            Destroy(runtimeRT);
            runtimeRT = null;
        }

        // ��ģ����ֶ������������µ�
        RenderTextureDescriptor desc = template
            ? template.descriptor
            : new RenderTextureDescriptor(1280, 960, RenderTextureFormat.ARGB32, 24);

        desc.msaaSamples = 1;
        desc.useMipMap = false;
        desc.autoGenerateMips = false;

        runtimeRT = new RenderTexture(desc) { name = "InternalRT_Runtime" };
        runtimeRT.Create();

        // ͬ֡�л��������� RawImage ��ͼ
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
