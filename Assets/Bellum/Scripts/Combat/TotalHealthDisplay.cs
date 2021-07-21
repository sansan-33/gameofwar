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
    [SerializeField] private EffectIcon[] buffPlayerEffects;
    [SerializeField] private EffectIcon[] buffEnemyEffects;

    private float currentPlayerTotalHealth = 0;
    private float currentEnemyTotalHealth = 0;
    float maxPlayerTotalHealth = 0;
    float maxEnemyTotalHealth = 0;
    RTSPlayer player;
    GameObject[] enemies;
    GameObject[] armies;
    Dictionary<string, EffectIcon> buffEffects = new Dictionary<string, EffectIcon>();

    private void Start() { 
        GameOverHandler.ClientOnGameOver += SetTotalHealthToDie;
        EffectStatus.ClientOnEffectUpdated += HandleEffectStatus;
        InitBuffEffects();
        if (NetworkClient.connection.identity == null) { return; }
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        YourName.text = "Player" + player.GetPlayerID();

        //Debug.Log($"TotalHealthDisplay.Start() TotalPlayerHealthBar.type:{TotalPlayerHealthBar.type}, TotalEnemyHealthBar.type:{TotalEnemyHealthBar.type}");
        if (player.GetTeamColor() == Color.red) {
            Sprite tmp = TotalPlayerHealthBar.sprite;
            TotalPlayerHealthBar.sprite = TotalEnemyHealthBar.sprite;
            TotalEnemyHealthBar.sprite = tmp;
            TotalPlayerHealthBar.type = Image.Type.Filled;
            TotalEnemyHealthBar.type = Image.Type.Filled;
            //Debug.Log($"TotalHealthDisplay.Start() inside if TotalPlayerHealthBar.type:{TotalPlayerHealthBar.type}, TotalEnemyHealthBar.type:{TotalEnemyHealthBar.type}");
        }
        //Debug.Log($"TotalHealthDisplay.Start() after if TotalPlayerHealthBar.type:{TotalPlayerHealthBar.type}, TotalEnemyHealthBar.type:{TotalEnemyHealthBar.type}");
    }
    private void InitBuffEffects()
    {
        int i = 0;
        foreach (EffectIcon effect in buffPlayerEffects)
        {
            buffEffects.Add(0 + "-" + effect.effectType.ToString(), effect);
            buffEffects.Add(1 + "-" + effect.effectType.ToString(), buffEnemyEffects[i++]);
        }
    }
    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= SetTotalHealthToDie;
        EffectStatus.ClientOnEffectUpdated -= HandleEffectStatus;
    }
    private void Update()
    {
        if (player is null) { return; }
        TotalPlayerHealthdisplay();
        TotalEnemyHealth();

    }
    private void TotalPlayerHealthdisplay()
    {
        currentPlayerTotalHealth = 0;
        if (armies is null || armies.Length == 0) { armies = GameObject.FindGameObjectsWithTag("King" + player.GetPlayerID()); }
        if (armies.Length == 0) return;
        PlayerName.text = "Player" + player.GetPlayerID();
        EnemyName.text = "Player" + player.GetEnemyID();
        float newProgress;
        foreach (GameObject army in armies)
        {
            if (army != null && army.TryGetComponent<Health>(out Health health))
                currentPlayerTotalHealth += health.getCurrentHealth();
            if (currentPlayerTotalHealth > maxPlayerTotalHealth)
            {
                maxPlayerTotalHealth = currentPlayerTotalHealth;
            }
        }
        newProgress = (float)currentPlayerTotalHealth / (float)maxPlayerTotalHealth;
        TotalPlayerHealthBar.fillAmount = newProgress;
        //Debug.Log($"TotalHealthDisplay.TotalPlayerHealthdisplay() TotalPlayerHealthBar.fillAmount:{TotalPlayerHealthBar.fillAmount}");
        TotalPlayerHealths.text = currentPlayerTotalHealth.ToString();
    }
    private void TotalEnemyHealth()
    {
        if(enemies is null || enemies.Length == 0) enemies = GameObject.FindGameObjectsWithTag("King" + player.GetEnemyID());
        if (enemies.Length == 0) return;

        currentEnemyTotalHealth = 0;
        float newProgress;
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null && enemy.TryGetComponent<Health>(out Health health))
                currentEnemyTotalHealth += health.getCurrentHealth();
            if (currentEnemyTotalHealth > maxEnemyTotalHealth)
            {
                maxEnemyTotalHealth = currentEnemyTotalHealth;
            }
        }
        newProgress = (float)currentEnemyTotalHealth / (float)maxEnemyTotalHealth;
        TotalEnemyHealths.text = currentEnemyTotalHealth.ToString();
        TotalEnemyHealthBar.fillAmount = newProgress;
        //Debug.Log($"TotalHealthDisplay.TotalEnemyHealth() TotalEnemyHealthBar.fillAmount:{TotalEnemyHealthBar.fillAmount}");

    }
    public void SetTotalHealthToDie(string winnerTag)
    {
        //Debug.Log($"Total Health Display ==> Player ID {player.GetPlayerID()} SetTotalHealthToDie {winnerTag}" );
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
    private void HandleEffectStatus(int playerid, UnitMeta.EffectType effectType, int value)
    {
        Debug.Log($"HandleEffectStatus playerid {playerid} , {effectType} / {value} ");
        buffEffects[playerid + "-" +  effectType.ToString()].gameObject.SetActive(value != 0);
    }
}
