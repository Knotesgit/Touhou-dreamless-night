using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public AudioClip BGM;
    public GameObject BGMName;
    [TextArea(2, 2)]
    public string speakerName;
    public Color nameColor = new Color(1,1,1,0);
    [TextArea(1, 3)]
    public string content;
    public Sprite portraitSprite;
    
    public bool isLeftSide = true;

    public float lineTimeInterval = 10f;
    public bool cannotSkip = false;
}