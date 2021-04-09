using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpCost : MonoBehaviour
{
    public int SPAmount = 0;
    private int MaxSpCost;

    private Image SPImage;
    private TMP_Text SPText;

    // Start is called before the first frame update
    void Start()
    {
        MaxSpCost = SPAmount;
        SPImage = GameObject.FindGameObjectWithTag("SP Bar").GetComponent<Image>();
        SPText = GameObject.FindGameObjectWithTag("SP Text").GetComponent<TextMeshProUGUI>();
    }
    public void UpdateSPAmount(int cost)
    {
        //Debug.Log("UpdateSPAmount");
        SPAmount += cost;
        SPText.text = (string)SPAmount.ToString();
        SPImage.fillAmount = (float)SPAmount / MaxSpCost;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
