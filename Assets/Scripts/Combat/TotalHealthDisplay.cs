using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TotalHealthDisplay : NetworkBehaviour
{
    [SerializeField] private Image TotalPlayerHealthBar = null;
    [SerializeField] private Image TotalEnemyHealthBar = null;
    [SerializeField] private TMP_Text TotalPlayerHealths = null;
    [SerializeField] private TMP_Text TotalEnemyHealths = null;
    [SerializeField] private TMP_Text PlayerName = null;
    [SerializeField] private TMP_Text EnemyName = null;
    [SerializeField] private TMP_Text YourName = null;

    private int militarySize = 0;
    private int EnermymilitarySize = 0;
    int MaxmilitarySize = 0;
    int MaxEnermymilitarySize = 0;
    private float progressImageVelocity;
    RTSPlayer player;


    private void Start()
    {
        GameOverHandler.ClientOnGameOver += SetTotalHealthToDie;
    }

    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= SetTotalHealthToDie;
    }

    public override void OnStartClient()
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        YourName.text = "Player" + player.GetPlayerID();
    }
    private void Update()
    {
        if (player is null) { return; }
        TotalPlayerHealthdisplay();
        TotalEnemyHealth();

    }
    private void TotalPlayerHealthdisplay()
    {

        militarySize = 0;
        GameObject[] armies = GameObject.FindGameObjectsWithTag("Player" + player.GetPlayerID());
        if (armies is null || armies.Length == 0) { return; }
        PlayerName.text = "Player" + player.GetPlayerID();
        EnemyName.text = "Player" + player.GetEnemyID();
        foreach (GameObject army in armies)
        {
            float newProgress;

            militarySize += army.GetComponent<Health>().getCurrentHealth();
            if (militarySize > MaxmilitarySize)
            {

                MaxmilitarySize = militarySize;
            }

            newProgress = (float)militarySize / (float)MaxmilitarySize;
            TotalPlayerHealthBar.fillAmount = newProgress;
            TotalPlayerHealths.text = militarySize.ToString();
        }

    }
    private void TotalEnemyHealth()
    {

        EnermymilitarySize = 0;
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Player" + player.GetEnemyID());
        foreach (GameObject EnermyArmy in enemies)
        {
            float newProgress;
            EnermymilitarySize += EnermyArmy.GetComponent<Health>().getCurrentHealth();
            if (EnermymilitarySize > MaxEnermymilitarySize)
            {
                MaxEnermymilitarySize = EnermymilitarySize;

            }

            newProgress = (float)EnermymilitarySize / (float)MaxEnermymilitarySize;
            TotalEnemyHealths.text = EnermymilitarySize.ToString();
            TotalEnemyHealthBar.fillAmount = newProgress;
        }

    }
    public void SetTotalHealthToDie(string winnerTag)
    {

        if (winnerTag == "Player1")
        {
            TotalPlayerHealthBar.fillAmount = 0f;
            TotalPlayerHealths.text = "0";
        }
        else
        {
            TotalEnemyHealths.text = "0";
            TotalEnemyHealthBar.fillAmount = 0f;
        }
    }

}
