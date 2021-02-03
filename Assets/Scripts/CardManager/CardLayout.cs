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
       
        if (cardindex == 0) { this.GetComponent<RectTransform>().anchoredPosition = new Vector3(-588, 206, 0); }
        if (cardindex == 1) { this.GetComponent<RectTransform>().anchoredPosition = new Vector3(-508, 206, 0); }
        if (cardindex == 2) { this.GetComponent<RectTransform>().anchoredPosition = new Vector3(-404, 206, 0); }
        if (cardindex == 3) { this.GetComponent<RectTransform>().anchoredPosition = new Vector3(-306, 206, 0); }
        if (cardindex == 4) { this.GetComponent<RectTransform>().anchoredPosition = new Vector3(-182, 206, 0); }
        if (cardindex == 5) { this.GetComponent<RectTransform>().anchoredPosition = new Vector3(-82, 206, 0); }
        if (Screen.height <= 3000 && Screen.width <= 2000)
        {
         
            this.GetComponent<RectTransform>().localScale = new Vector3((float)0.3, (float)0.3, (float)0.3);

            RectTransform rt = cardPrefab.GetComponent<RectTransform>();
        
            if (cardindex == 4)
            {
                cardOffset = 50;
            }else if (cardindex == 5)
            {
                cardOffset = 53;
            }
            rt.anchoredPosition = new Vector3(-288, 160, 0) + new Vector3(cardindex * cardOffset, 0, 0);
            cardOffset = 40;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
