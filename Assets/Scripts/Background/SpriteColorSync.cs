using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteColorSync : MonoBehaviour
{
    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        sr.material.SetColor("_Color", sr.color);
        sr.material.SetFloat("_TimeValue", Time.time);
    }
}
