using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AudioEvent
{
    public float time;               // ����ʱ��
    public AudioClip clip;          // ��Ƶ��Դ
    public float volume = 1f;       // ����
    public bool loop = false;       // �Ƿ�ѭ��
    public bool useOneShot = false; // �Ƿ�ʹ�� PlayOneShot������Ч��

    [SerializeField, HideInInspector]
    public string groupName = "";
}