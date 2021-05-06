using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TotalHealthDisplay : MonoBehaviour
{
    [SerializeField] private Image TotalPlayerHealthBar = null;
    [SerializeField] private Image TotalEnemyHealthBar = null;
    [SerializeField] private TMP_Text TotalPlayerHealths = null;
    [SerializeField] private TMP_Text TotalEnemyHealths = null;
    [SerializeField] private TMP_Text PlayerName = null;
    [SerializeField] private TMP_Text EnemyName = null;
    [SerializeField] private TMP_Text YourName = null;

    private float militarySize = 0;
    private float EnermymilitarySize = 0;
    float MaxmilitarySize = 0;
    float MaxEnermymilitarySize = 0;
    RTSPlayer player;
    GameObject[] enemies;
    GameObject[] armies;

    private void Start() { 
        GameOverHandler.ClientOnGameOver += SetTotalHealthToDie;
        if (NetworkClient.connection.identity == null) { return; }
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        YourName.text = "Player" + player.GetPlayerID();
        if (player.GetTeamColor() == Color.red) {
            Sprite tmp = TotalPlayerHealthBar.sprite;
            TotalPlayerHealthBar.sprite = TotalEnemyHealthBar.sprite;
            TotalEnemyHealthBar.sprite = tmp;
            TotalPlayerHealthBar.type = Image.Type.Filled;
            TotalEnemyHealthBar.type = Image.Type.Filled;
        }
    }
    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= SetTotalHealthToDie;
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
        if (armies is null || armies.Length == 0) { armies = GameObject.FindGameObjectsWithTag("King" + player.GetPlayerID()); }
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
        if(enemies is null || enemies.Length == 0) enemies = GameObject.FindGameObjectsWithTag("King" + player.GetEnemyID());
        EnermymilitarySize = 0;
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
        Debug.Log($"Total Health Display ==> Player ID {player.GetPlayerID()} SetTotalHealthToDie {winnerTag}" );
        if (winnerTag == UnitMeta.REDTEAM)
        {
            TotalPlayerHealthBar.fillAmount = 0f;
            TotalPlayerHealthBar.type = Image.Type.Filled;
            TotalPlayerHealths.text = "0";
        }
        else
        {
            TotalEnemyHealths.text = "0";
            TotalPlayerHealthBar.type = Image.Type.Filled;
            TotalEnemyHealthBar.fillAmount = 0f;
        }
    }

}
