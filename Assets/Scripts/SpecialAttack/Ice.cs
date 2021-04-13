using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class Ice : MonoBehaviour
{
    [SerializeField] public ParticleSystem iceEffect;
    [SerializeField] private LayerMask layerMask = new LayerMask();
    public List<GameObject> enemyList = new List<GameObject>();
    private Button SPButton;
    private RTSPlayer player;
    private GameObject hitCollider;
    private Transform searchPoint;
    private SpCost spCost;

    public int SPCost = 10;
    public int IceDamage;
    public int attackRange = 100;
    public float UnFrezzeTimer = 3;
    private float UnFrezzeTime;
    public int buttonTicket;
    private bool SpawnedButton;
    private bool IsFrezze = false;
    void Start()
    {
        Health.IceHitUpdated += IceBreak;
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        if (CompareTag("King" + player.GetEnemyID()) || CompareTag("Player" + player.GetEnemyID())) { return; }
        spCost = FindObjectOfType<SpCost>();
        //Instantiate SpButton and is it already spawned
        SpawnedButton = FindObjectOfType<SpButton>().InstantiateSpButton(SpecialAttackDict.SpecialAttackType.Ice, GetComponent<Unit>());
        //SpButton will give unit a int to get back the button that it spwaned
        if (SpawnedButton) { SPButton = FindObjectOfType<SpButton>().GetButton(GetComponent<Unit>().SpBtnTicket).GetComponent<Button>(); }
        if (SPButton == null) { return; }
        SPButton.onClick.RemoveAllListeners();
        SPButton.onClick.AddListener(OnPointerDown);
        searchPoint = transform;
        UnFrezzeTime = UnFrezzeTimer;
    }
    private void OnDestroy()
    {
        foreach (GameObject unit in enemyList)
        {
            IceBreak(unit);
        }
        Health.IceHitUpdated -= IceBreak;
    }
    public void OnPointerDown()
    {
       
        enemyList.Clear();
        UnitRepeatAttackDelaykeys.Clear();
        UnitSpeedkeys.Clear();
        //if(spCost.SPAmount < SPCost) {return;}
        spCost.UpdateSPAmount(-SPCost);

        GameObject closestTarget = null;
        bool haveTarget = true;
        var distance = float.MaxValue;
        var localDistance = 0f;
        //distanceList.Clear();
        //targetList.Clear();
        
            //bool findedTarget = false;
            //Search target in a distance
            Collider[] hitColliders = Physics.OverlapBox(searchPoint.position, transform.localScale * attackRange, Quaternion.identity, layerMask);
            int i = 0;
            while (i < hitColliders.Length)
            {
                distance = float.MaxValue;
                hitCollider = hitColliders[i++].transform.gameObject;
                // check If the target is cloestest to king && it is not in the same team && check if it already finded the target
                if ((localDistance = (hitCollider.transform.position - transform.position).sqrMagnitude) < distance)
                {
                    int id = ((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1 ? 1 : player.GetPlayerID() == 0 ? 1 : 0;
                    if (hitCollider.CompareTag("Player" + id) || hitCollider.CompareTag("King" + id))
                    {
                    //if (localDistance > minAttackRange)
                    // {
                    //findedTarget = true;
                    //distance = localDistance;
                    //closestTarget = hitCollider;
                    //StopTacticalBehavior while using Special Attack
                    //Debug.Log(hitCollider);
                    enemyList.Add(hitCollider);
                    // hitCollider.GetComponent<Health>().DealDamage(IceDamage);
                    hitCollider.GetComponent<Health>().IsFrezze = true;
                    CardStats cardStats = hitCollider.GetComponent<CardStats>();
                    UnitRepeatAttackDelaykeys.Add(hitCollider, cardStats.repeatAttackDelay);
                    UnitSpeedkeys.Add(hitCollider, cardStats.speed);
                    hitCollider.GetComponent<UnitPowerUp>().CmdPowerUp(hitCollider, cardStats.star, cardStats.cardLevel, cardStats.health, cardStats.attack, Mathf.Infinity, 0, cardStats.defense, cardStats.special);
                    Instantiate(iceEffect, hitCollider.transform);
                        // Move the searchPoint to the next target, so it will not search at the same point
                        //searchPoint = closestTarget.transform;
                        // }
                }
                }
            }
        IsFrezze = true;
            // if there is no more target is finded then break


        /*searchPoint = attackPoint.transform;
        // if it doesnot find any target return
        if (closestTarget == null) { return; }
        for (int a = 0; a < targetList.ToArray().Length; a++)
        {

            AttackTarget(distanceList.ToArray()[a], targetList.ToArray()[a].transform);
        }
        GetComponent<UnitWeapon>().ReScaleDamageDeal();*/

}
    private void Update()
    {
        if(UnFrezzeTimer > 0 && IsFrezze == true)
        {
            UnFrezzeTimer -= Time.deltaTime;
        }
        else if(IsFrezze == true)
        {
            UnFrezzeTimer = UnFrezzeTime;
            IsFrezze = false;
            foreach (GameObject unit in enemyList)
            {
                IceBreak(unit);
            }
        }
        if(IsFrezze == true)
        {
            foreach (GameObject unit in enemyList)
            {
                Instantiate(iceEffect, unit.transform);
            }
        }
    }
    private void IceBreak(GameObject unit)
    {
        if(unit == null) { return; }
        unit.GetComponent<Health>().IsFrezze = false;
        UnitRepeatAttackDelaykeys.TryGetValue(unit, out float repeatAttackDelay);
        UnitSpeedkeys.TryGetValue(unit, out int speed);
        CardStats cardStats = unit.GetComponent<CardStats>();
        unit.GetComponent<UnitPowerUp>().CmdPowerUp(hitCollider, cardStats.star, cardStats.cardLevel, cardStats.health, cardStats.attack, repeatAttackDelay, speed, cardStats.defense, cardStats.special);
    }
    public float GetUnitRepeatAttackDelaykeys(GameObject unit)
    {
        UnitRepeatAttackDelaykeys.TryGetValue(unit, out float repeatAttackDelay);
        return repeatAttackDelay;
    }
    public int GetUnitSpeedkeys(GameObject unit)
    {
        UnitSpeedkeys.TryGetValue(unit, out int speed);
        return speed;
    }
    public static Dictionary<GameObject, float> UnitRepeatAttackDelaykeys = new Dictionary<GameObject, float>()
    {

    };
    public static Dictionary<GameObject, int> UnitSpeedkeys = new Dictionary<GameObject, int>()
    {

    };
}
