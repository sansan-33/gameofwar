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
    public int EnemyFrezzeTime = 3;
    
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
        //Debug.Log("OnPointerDown");
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
                unit.GetComponent<UnitMovement>().CmdStop();
                enemyReFightTimer = EnemyFrezzeTime;
                IsFrezzing = true;
                CanUnFrezze = true;
                TB.HandleStun(true);
                TB.StopAllTacticalBehavior(player.GetPlayerID());
            }
        }
        else // Multi player seneriao
        {
            //Debug.Log($"OnPointerDown Defend SP Multi shieldList {shieldList.Length}");
            foreach (GameObject unit in enemyList)
            {  // Only Set on our side

                //stop enenmy
               
                    unit.GetComponent<UnitMovement>().CmdStop();
                    enemyReFightTimer = EnemyFrezzeTime;
                
                IsFrezzing = true;
                CanUnFrezze = true;
                TB.HandleStun(true);
                TB.StopAllTacticalBehavior(player.GetPlayerID());
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
            Debug.Log("Awake");
            //IsFrezzing = false;
            //TB.HandleStun(false);
            //CanUnFrezze = false;
            //StartCoroutine(TB.TacticalFormation(player.GetPlayerID(), player.GetEnemyID()));
        }
       if(IsFrezzing == true)
       {
            foreach (GameObject unit in enemyList)
            {
                
                //Debug.Log($"Stop unit tag is {unit.name}");
                //unit.GetComponent<UnitMovement>().CmdStop(); 
            }
        }
    }
}
