using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardLayout : MonoBehaviour
{
    [SerializeField] int cardindex;
    [SerializeField] Button cardPrefab;
    private float cardOffset=40;
    // Start is called before the first frame update
    void Start()
    {
        int y = 300;
        //if (cardindex == 0) { this.GetComponent<RectTransform>().anchoredPosition = new Vector3(-600, y, 0); }
        //if (cardindex == 1) { this.GetComponent<RectTransform>().anchoredPosition = new Vector3(-500, y, 0); }
        //if (cardindex == 2) { this.GetComponent<RectTransform>().anchoredPosition = new Vector3(-400, y, 0); }
        //if (cardindex == 3) { this.GetComponent<RectTransform>().anchoredPosition = new Vector3(-300, y, 0); }
        //if (cardindex == 4) { this.GetComponent<RectTransform>().anchoredPosition = new Vector3(-200, y, 0); }
        //if (cardindex == 5) { this.GetComponent<RectTransform>().anchoredPosition = new Vector3(-100, y, 0); }
        if (cardindex == 6) { this.GetComponent<RectTransform>().anchoredPosition = new Vector3(-60, y, 0); }
        if (cardindex == 7) { this.GetComponent<RectTransform>().anchoredPosition = new Vector3(-160, y, 0); }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
