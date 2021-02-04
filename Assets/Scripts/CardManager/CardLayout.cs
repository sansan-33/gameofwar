using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardLayout : MonoBehaviour
{
    [SerializeField] int cardindex;
    [SerializeField] int x;
    [SerializeField] int y=350;
    [SerializeField] Button cardPrefab;
    private float cardOffset=40;
    // Start is called before the first frame update
    void Start()
    {
       
        if (cardindex == 0) { this.GetComponent<RectTransform>().anchoredPosition = new Vector3(-860, y, 0); }
        if (cardindex == 1) { this.GetComponent<RectTransform>().anchoredPosition = new Vector3(-737, y, 0); }
        if (cardindex == 2) { this.GetComponent<RectTransform>().anchoredPosition = new Vector3(-567, y, 0); }
        if (cardindex == 3) { this.GetComponent<RectTransform>().anchoredPosition = new Vector3(-410, y, 0); }
        if (cardindex == 4) { this.GetComponent<RectTransform>().anchoredPosition = new Vector3(-223, y, 0); }
        if (cardindex == 5) { this.GetComponent<RectTransform>().anchoredPosition = new Vector3(-67, y, 0); }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
