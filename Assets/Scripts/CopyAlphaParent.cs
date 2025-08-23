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
            Debug.LogWarning("CopyAlphaFromTMPParent: û�и����塣");
            enabled = false;
            return;
        }

        parentTMP = transform.parent.GetComponent<TextMeshPro>();
        if (parentTMP == null)
        {
            Debug.LogWarning("CopyAlphaFromTMPParent: �����岻�� TextMeshPro��");
        }
        
        parentSprite = transform.parent.GetComponent<SpriteRenderer>();
        if (parentSprite == null)
        {
            Debug.LogWarning("CopyAlphaFromTMPParent: �����岻�� Spriterender��");
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
