using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateEffect : MonoBehaviour
{
    // Start is called before the first frame update
    public float rotationSpeed = 90f; 
    public bool clockwise = true;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float angle = rotationSpeed * Time.deltaTime * (clockwise ? -1 : 1);
        transform.Rotate(0f, 0f, angle);
    }
}
