using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BackgroundEvent
{
    public float time; // 事件触发时间（单位秒）

    [Header("Background setting")]
    public Sprite backgroundSprite;
    public bool enableScroll = false;
    public Vector2 scrollSpeed = Vector2.zero;

    [Header("Fading control")]
    public bool enableFade = false;
    public float fadeToAlpha = 1f;
    public float fadeDuration = 1f;

    [Header("Rotation control")]
    public bool enableRotation = false;
    public float rotateToAngle = 0f;
    public float rotateDuration = 0f;

    [Header("Fog control")]
    public bool toggleFog = false;

    [SerializeField, HideInInspector]
    public string groupName = "";
}