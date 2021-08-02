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
    [SerializeField] TMP_Text eleixerValue;
    [SerializeField] TMP_Text enemyEleixerValue;

    public int maxEleixer = 10;
    public float maxEleixerTimer;
    private float eleixerTimer = 2f;
    public int eleixer = 0;
    public int enemyEleixer = 0;
    bool IS_SPEEDUP = false;
    public static event Action<int> UpdateEnemyElexier;
    // Start is called before the first frame update
    void Start()
    {
        maxEleixerTimer = eleixerTimer;
        GameStartDisplay.ServerGameSpeedUp += speedUpEleixier;
    }
    private void OnDestroy()
    {
        GameStartDisplay.ServerGameSpeedUp -= speedUpEleixier;
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
        eleixerValue.text = eleixer.ToString();
        eleixerBarImage.fillAmount = (float)eleixer / (float)maxEleixer;
        if (enemyeleixerBarImage != null)
        {
            enemyEleixerValue.text = enemyEleixer.ToString();
            enemyeleixerBarImage.fillAmount = (float)enemyEleixer / (float)maxEleixer;
        }
    }
    public void speedUpEleixier()
    {
        if (IS_SPEEDUP) { return; }
        maxEleixerTimer = maxEleixerTimer / 2f;
        eleixerSpeed.text = "X 3";
        IS_SPEEDUP = true;
    }
}
