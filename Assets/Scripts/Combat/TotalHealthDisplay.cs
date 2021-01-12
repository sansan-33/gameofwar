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
    int MaxmilitarySize = 0;
    int MaxEnermymilitarySize = 0;
    RTSPlayer player;
    string PLAYERTAG = "";
    string ENEMYTAG = "";

    public override void OnStartClient()
    {
        Debug.Log("On start client ... ");
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        PLAYERTAG = "Player" + player.GetPlayerID();
        ENEMYTAG = "Player" + player.GetEnemyID();
    }
    private void Update()
    {
        TotalPlayerHealthdisplay();
        TotalEnemyHealth();
    }
    private void TotalPlayerHealthdisplay()
    {
        militarySize = 0;
        GameObject[] armies = GameObject.FindGameObjectsWithTag(PLAYERTAG);
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
    private void TotalEnemyHealth()
    {
        //Debug.Log($"ENEMYTAG {ENEMYTAG}");
        EnermymilitarySize = 0;
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(ENEMYTAG);
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

    }
}

