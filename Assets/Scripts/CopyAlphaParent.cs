using TMPro;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CopyAlphaParent : MonoBehaviour
{
    private SpriteRenderer selfRenderer;
    private SpriteRenderer parentSprite;
    private TextMeshPro parentTMP;
    
    void Start()
    {
        selfRenderer = GetComponent<SpriteRenderer>();

        if (transform.parent == null)
        {
            Debug.LogWarning("CopyAlphaFromTMPParent: 没有父物体。");
            enabled = false;
            return;
        }

        parentTMP = transform.parent.GetComponent<TextMeshPro>();
        if (parentTMP == null)
        {
            Debug.LogWarning("CopyAlphaFromTMPParent: 父物体不是 TextMeshPro。");
        }
        
        parentSprite = transform.parent.GetComponent<SpriteRenderer>();
        if (parentSprite == null)
        {
            Debug.LogWarning("CopyAlphaFromTMPParent: 父物体不是 Spriterender。");
        }
    }

    void Update()
    {
        if (selfRenderer == null || (parentTMP == null && parentSprite == null))
            return;
        if (parentTMP != null)
        {
            Color color = selfRenderer.color;
            color.a = parentTMP.color.a;
            selfRenderer.color = color;
        }
        if (parentSprite != null) 
        {
            Color color = selfRenderer.color;
            color.a = parentSprite.color.a;
            selfRenderer.color = color;
        }
        
    }
}
