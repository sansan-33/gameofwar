using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class Stun : MonoBehaviour
{
    private List<GameObject> enemyList;
    private TacticalBehavior TB;
    private Button SPButton;
    private RTSPlayer player;
    private SpCost spCost;

    private float enemyReFightTimer = -10000;
    public float SPCost = 10;
    public int buttonTicket;
    private bool SpawnedButton;
    private bool IsFrezzing;
    private bool CanUnFrezze = false;
    public int EnemyFrezzeTime = 5;
    
    // Start is called before the first frame update
    void Start()
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        spCost = FindObjectOfType<SpCost>();
        //Instantiate SpButton and is it already spawned
        SpawnedButton = FindObjectOfType<SpButton>().InstantiateSpButton(SpecialAttackDict.SpecialAttackType.Stun, GetComponent<Unit>());
        //SpButton will give unit a int to get back the button that it spwaned
        if (SpawnedButton) { SPButton = FindObjectOfType<SpButton>().GetButton(GetComponent<Unit>().SpBtnTicket).GetComponent<Button>(); }
        if (SPButton == null) { return; }
        SPButton.onClick.RemoveAllListeners();
        SPButton.onClick.AddListener(OnPointerDown);
        TB = GameObject.FindGameObjectWithTag("TacticalSystem").GetComponent<TacticalBehavior>();
    }
    public void OnPointerDown()
    {
        Debug.Log("OnPointerDown");
        spCost.SPAmount -= (int)SPCost;

        //find all unit
        enemyList = GameObject.FindGameObjectsWithTag("Player" + player.GetEnemyID()).ToList();

        var a = GameObject.FindGameObjectsWithTag("King" + player.GetEnemyID());
        if (a.Length > 0)
            enemyList.AddRange(a);
       
        if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)//1 player mode
        {
           //Debug.Log("One Player Mide");
            foreach (GameObject unit in enemyList)
            {  // Only Set on our side
                //Debug.Log($"Tag -- > {unit.tag}");
                
                    //stop enenmy
                    //Debug.Log("stop");
                //unit.GetComponent<UnitMovement>().CmdStop();
                enemyReFightTimer = EnemyFrezzeTime;
                //IsFrezzing = true;
                CanUnFrezze = true;
                //unit.GetComponent<UnitMovement>().CmdStun();
                CardStats cardStats = unit.GetComponent<CardStats>();
                UnitRepeatAttackDelaykeys.Add(unit, cardStats.repeatAttackDelay);
                UnitSpeedkeys.Add(unit, cardStats.speed);
                unit.GetComponent<UnitPowerUp>().CmdPowerUp(unit, cardStats.star, cardStats.cardLevel, cardStats.health, cardStats.attack, Mathf.Infinity, 0, cardStats.defense, cardStats.special);
                
                //TB.StopAllTacticalBehavior(player.GetEnemyID());
            }
        }
        else // Multi player seneriao
        {
            //Debug.Log($"OnPointerDown Defend SP Multi shieldList {shieldList.Length}");
            // Only Set on our side

            //stop enenmy
            foreach (GameObject unit in enemyList)
            {
                //unit.GetComponent<UnitMovement>().CmdStop();
                enemyReFightTimer = EnemyFrezzeTime;
                //IsFrezzing = true;
                CanUnFrezze = true;
                //unit.GetComponent<UnitMovement>().CmdStun();
                CardStats cardStats = unit.GetComponent<CardStats>();
                UnitRepeatAttackDelaykeys.Add(unit, cardStats.repeatAttackDelay);
                UnitSpeedkeys.Add(unit, cardStats.speed);
                unit.GetComponent<UnitPowerUp>().CmdPowerUp(unit, cardStats.star, cardStats.cardLevel, cardStats.health, cardStats.attack, Mathf.Infinity, 0, cardStats.defense, cardStats.special);
                Debug.Log($"stop {unit.name}");
                //TB.StopAllTacticalBehavior(player.GetEnemyID());
            }
        }
    }
    /*public bool Between(int num, int lower, int upper)
    {
        if(num > lower&& num < upper)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void AwakeEnemy()
    {
        TB.TryReinforce(null);
    }*/
        // Update is called once per frame
    void Update()
    {
        if(enemyReFightTimer > 0)
        {
            //Debug.Log($"Stop {enemyReFightTimer},tag is {tag}");
            enemyReFightTimer -= Time.deltaTime;
        }
        else if(CanUnFrezze == true)
        {
            foreach (GameObject unit in enemyList)
            {
                
                CardStats cardStats = unit.GetComponent<CardStats>();
                UnitRepeatAttackDelaykeys.TryGetValue(unit, out float repeatAttackDelay);
                UnitSpeedkeys.TryGetValue(unit, out int speed);
                unit.GetComponent<UnitPowerUp>().CmdPowerUp(unit, cardStats.star, cardStats.cardLevel, cardStats.health, cardStats.attack, repeatAttackDelay, speed, cardStats.defense, cardStats.special);
                //Debug.Log($"Awake {repeatAttackDelay}, {speed}");
            }
            // IsFrezzing = false;
            CanUnFrezze = false;
            //StartCoroutine(TB.TacticalFormation(player.GetPlayerID(), player.GetEnemyID()));
        }    
    }
    public static Dictionary<GameObject, float> UnitRepeatAttackDelaykeys = new Dictionary<GameObject, float>()
    {

    };
    public static Dictionary<GameObject, int> UnitSpeedkeys = new Dictionary<GameObject, int>()
    {

    };
}
