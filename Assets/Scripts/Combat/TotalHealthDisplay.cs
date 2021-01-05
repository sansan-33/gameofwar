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
    private int militarySize = 0;
    private int EnermymilitarySize = 0;
    float unitTimer = 1;
    int MaxmilitarySize = 1;
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

        militarySize = 1;

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
        }


    }
    private void totalEnermyhealth()
    {

        if (militarySize > 0)
        {
            EnermymilitarySize = 0;
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject EnermyArmy in enemies)
        {
            float newProgress;
            EnermymilitarySize += EnermyArmy.GetComponent<Health>().getCurrentHealth();
            if (EnermymilitarySize > MaxEnermymilitarySize)
            {
                MaxEnermymilitarySize = EnermymilitarySize;

            }

            newProgress = MaxEnermymilitarySize / EnermymilitarySize;

            TotalEnermyhealth.fillAmount = Mathf.SmoothDamp(
                TotalEnermyhealth.fillAmount,
                newProgress,
                ref progressImageVelocity,
                0.1f);
        }

        b++;
    }


}

