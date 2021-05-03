using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static TacticalBehavior;
using System;

public class eleixier : MonoBehaviour
{
    [SerializeField] TMP_Text eleixerTimerImage;
    public int maxEleixer = 10;
    public float maxEleixerTimer;
    private float eleixerTimer = 4f;
    //public static Action<int> UpdateEleixer;
    public int eleixer = 0;
    // Start is called before the first frame update
    void Start()
    {
        maxEleixerTimer = eleixerTimer;
    }

    // Update is called once per frame
    private void Update()
    {
        eleixerTimer -= Time.deltaTime;
        if (eleixerTimer <= 0)
        {
            eleixerTimer = maxEleixerTimer;
            if (eleixer < maxEleixer)
            {
                eleixer += 1;
                
            }
        }
    }
    public void speedUpEleixier(BehaviorSelectionType selectionType)
    {
       
        if (selectionType== BehaviorSelectionType.Attack|| selectionType == BehaviorSelectionType.Flank)
        {
            maxEleixerTimer = 2f;
            eleixerTimerImage.text = "X 2 eleixer";
        }
        else
        {
            maxEleixerTimer = 4f;
            eleixerTimerImage.text = "X 1 eleixer";
        }
       

    }
}
