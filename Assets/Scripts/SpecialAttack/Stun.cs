using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class Stun : MonoBehaviour
{
    private UnitWeapon UnitWeapon;
    private List<GameObject> enemyList;
    private TacticalBehavior TB;
    private Button SPButton;
    private RTSPlayer player;
    private SpCost spCost;

    private float enemyReFightTimer = -10000;
    public int SPCost = 10;
    public int buttonTicket;
    private bool SpawnedButton;
    private bool CanUnFrezze = false;
    private bool CMVirtualIsOn = false;
    public int EnemyFrezzeTime = 5;
    
    // Start is called before the first frame update
    void Start()
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        if (CompareTag("King" + player.GetEnemyID()) || CompareTag("Player" + player.GetEnemyID())) { return; }
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
        if (spCost.SPAmount < SPCost) { return; }
        spCost.UpdateSPAmount(-SPCost);
        UnitRepeatAttackDelaykeys.Clear();
        UnitSpeedkeys.Clear();
        //find all enemy unit
        enemyList = GameObject.FindGameObjectsWithTag("Player" + player.GetEnemyID()).ToList();

        var a = GameObject.FindGameObjectsWithTag("King" + player.GetEnemyID());
        if (a.Length > 0)
            enemyList.AddRange(a);
       
        if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)//1 player mode
        {
            //stop enenmy
            foreach (GameObject unit in enemyList)
            {  
                enemyReFightTimer = EnemyFrezzeTime;
                CanUnFrezze = true;
                CardStats cardStats = unit.GetComponent<CardStats>();
                UnitRepeatAttackDelaykeys.Add(unit, cardStats.repeatAttackDelay);
                UnitSpeedkeys.Add(unit, cardStats.speed);
                unit.GetComponent<UnitPowerUp>().CmdPowerUp(unit, cardStats.star, cardStats.cardLevel, (int)unit.GetComponent<Health>().getCurrentHealth(), cardStats.attack, Mathf.Infinity, 0, cardStats.defense, cardStats.special);
                if (unit.TryGetComponent<UnitWeapon>(out UnitWeapon unitWeapon) && CMVirtualIsOn == false)
                {
                    CMVirtualIsOn = true;
                    unitWeapon.CMVirtualIsOn = true;
                    UnitWeapon = unitWeapon;
                }
            }
        }
        else // Multi player seneriao
        {
            //stop enenmy
            foreach (GameObject unit in enemyList)
            {
                enemyReFightTimer = EnemyFrezzeTime;
                CanUnFrezze = true;
                CardStats cardStats = unit.GetComponent<CardStats>();
                UnitRepeatAttackDelaykeys.Add(unit, cardStats.repeatAttackDelay);
                UnitSpeedkeys.Add(unit, cardStats.speed);
                unit.GetComponent<UnitPowerUp>().CmdPowerUp(unit, cardStats.star, cardStats.cardLevel, (int)unit.GetComponent<Health>().getCurrentHealth(), cardStats.attack, Mathf.Infinity, 0, cardStats.defense, cardStats.special);
                if (unit.TryGetComponent<UnitWeapon>(out UnitWeapon unitWeapon) && CMVirtualIsOn == false)
                {
                    CMVirtualIsOn = true;
                    unitWeapon.CMVirtualIsOn = true;
                    UnitWeapon = unitWeapon;
                }
            }
        }
    }
        // Update is called once per frame
    void Update()
    {
        if(enemyReFightTimer > 0)
        {
            enemyReFightTimer -= Time.deltaTime;
        }
        else if(CanUnFrezze == true)
        {
            foreach (GameObject unit in enemyList)
            {
                
                    UnitWeapon.CMVirtualIsOn = false;
                CardStats cardStats = unit.GetComponent<CardStats>();
                UnitRepeatAttackDelaykeys.TryGetValue(unit, out float repeatAttackDelay);
                UnitSpeedkeys.TryGetValue(unit, out int speed);
                unit.GetComponent<UnitPowerUp>().CmdPowerUp(unit, cardStats.star, cardStats.cardLevel, (int)unit.GetComponent<Health>().getCurrentHealth(), cardStats.attack, repeatAttackDelay, speed, cardStats.defense, cardStats.special);
            }
            CanUnFrezze = false;
        }    
    }
    public static Dictionary<GameObject, float> UnitRepeatAttackDelaykeys = new Dictionary<GameObject, float>()
    {

    };
    public static Dictionary<GameObject, int> UnitSpeedkeys = new Dictionary<GameObject, int>()
    {

    };
}
