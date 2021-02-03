using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesLayout : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (Screen.height <= 3000 && Screen.width <= 2000)
        {

            
            RectTransform rt = this.GetComponent<RectTransform>();


            rt.anchoredPosition = new Vector3(-350, 30, 0);
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
