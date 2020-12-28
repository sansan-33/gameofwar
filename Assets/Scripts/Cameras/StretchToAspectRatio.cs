using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StretchToAspectRatio : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newScale = transform.localScale;
        newScale.x = Camera.main.aspect;
        transform.localScale = newScale;

    }
}
