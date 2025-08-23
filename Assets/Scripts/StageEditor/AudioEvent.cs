using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AudioEvent
{
    public float time;               // 触发时间
    public AudioClip clip;          // 音频资源
    public float volume = 1f;       // 音量
    public bool loop = false;       // 是否循环
    public bool useOneShot = false; // 是否使用 PlayOneShot（短音效）

    [SerializeField, HideInInspector]
    public string groupName = "";
}