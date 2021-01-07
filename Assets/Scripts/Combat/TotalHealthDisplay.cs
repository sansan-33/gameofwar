using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TotalHealthDisplay : NetworkBehaviour
{
    [SerializeField] private Image TotalPlayerhealth = null;
    [SerializeField] private Image TotalEnermyhealth = null;
    [SerializeField] private TMP_Text TotalPlayerhealths = null;
    [SerializeField] private TMP_Text TotalEnermyhealths = null;
    private int militarySize = 0;
    private int EnermymilitarySize = 0;
    float unitTimer = 1;
    int MaxmilitarySize = 0;
    int a = 0;
    int MaxEnermymilitarySize = 0;
    int b = 0;
    private float progressImageVelocity;
    private void Update()
    {

        TotalPlayerHealthdisplay();
        totalEnermyhealth();

    }
    private void UnitTimer()
    {

        unitTimer += Time.deltaTime;
    }
    private void TotalPlayerHealthdisplay()
    {

        militarySize = 0;

        GameObject[] armies = GameObject.FindGameObjectsWithTag("Player");



        foreach (GameObject army in armies)
        {
            float newProgress;

            militarySize += army.GetComponent<Health>().getCurrentHealth();
            if (militarySize > MaxmilitarySize)
            {

                MaxmilitarySize = militarySize;
            }
           
            newProgress = (float) militarySize / (float) MaxmilitarySize;
            TotalPlayerhealth.fillAmount = newProgress;
            TotalPlayerhealths.text = militarySize.ToString();
        }


    }
    private void totalEnermyhealth()
    {

        
            EnermymilitarySize = 0;
        

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject EnermyArmy in enemies)
        {
            float newProgress;
            EnermymilitarySize += EnermyArmy.GetComponent<Health>().getCurrentHealth();
            if (EnermymilitarySize > MaxEnermymilitarySize)
            {
                MaxEnermymilitarySize = EnermymilitarySize;

            }
         
            newProgress = (float)EnermymilitarySize / (float)MaxEnermymilitarySize;
            TotalEnermyhealths.text = EnermymilitarySize.ToString();
            TotalEnermyhealth.fillAmount = newProgress;
        }

        b++;
    }


}

