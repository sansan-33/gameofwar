using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static TacticalBehavior;
using System;

public class TotalEleixier : MonoBehaviour
{
    [SerializeField] TMP_Text eleixerSpeed;
    [SerializeField] public Image eleixerBarImage = null;
    [SerializeField] public Image enemyeleixerBarImage = null;

    public int maxEleixer = 10;
    public float maxEleixerTimer;
    private float eleixerTimer = 4f;
    public int eleixer = 0;
    public int enemyEleixer = 0;
    public static event Action<int> UpdateEnemyElexier;
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
            if(enemyEleixer < maxEleixer)
            {
                enemyEleixer += 1;
                UpdateEnemyElexier?.Invoke(enemyEleixer);
            }
            //Debug.Log(enemyEleixer);
        }
        eleixerBarImage.fillAmount = (float)eleixer / (float)maxEleixer;
        if (enemyeleixerBarImage != null)
        {
            enemyeleixerBarImage.fillAmount = (float)enemyEleixer / (float)maxEleixer;
        }
    }
    public void speedUpEleixier(BehaviorSelectionType selectionType)
    {
       
        if (selectionType== BehaviorSelectionType.Attack|| selectionType == BehaviorSelectionType.Flank)
        {
            maxEleixerTimer = 2f;
            eleixerSpeed.text = "X 2 eleixer";
        }
        else
        {
            maxEleixerTimer = 4f;
            eleixerSpeed.text = "X 1 eleixer";
        }
       

    }
}
