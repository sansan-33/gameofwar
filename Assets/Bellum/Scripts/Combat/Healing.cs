using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Healing : NetworkBehaviour
{
    private RTSPlayer player;
    private GameObject[] capsules;
    private int healingRange = 25;
    private int repeatHealingDelay = 1;
    private float lastHealingTime;
    private int healingAmount = 1;
    private bool HEALING_ENABLED = false;
    public GameObject healingPrefab;
    List<GameObject> armies = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        lastHealingTime = Time.time + 5f;
    }
    // Update is called once per frame
   
    private void Update()
    {
        if (!HEALING_ENABLED) { return; }

        if (this.isLocalPlayer) { return; }

        if (!hasAuthority){return;}

        if (lastHealingTime + repeatHealingDelay > Time.time) { return; }
        /*
        capsules = GameObject.FindGameObjectsWithTag("PlayerBase" + player.GetPlayerID());

        if (tb.GetBehaviorSelectionType(player.GetPlayerID()) != TacticalBehavior.BehaviorSelectionType.Defend) {
            foreach (GameObject capsule in capsules)
            {
                cmdHealingPrefab(capsule, false);
                //Debug.Log($"Disbale capsule {capsule.name}");
            }
            return;
        }
        */

        GameObject[] units = GameObject.FindGameObjectsWithTag("Player" + player.GetPlayerID());
        GameObject king = GameObject.FindGameObjectWithTag("King" + player.GetPlayerID());
        GameObject[] provokeTanks = GameObject.FindGameObjectsWithTag("Provoke" + player.GetPlayerID());
        armies = new List<GameObject>();
        if(units != null && units.Length > 0)
        armies = units.ToList();
        if (king != null)
            armies.Add(king);
        if (provokeTanks != null && provokeTanks.Length > 0)
            armies.AddRange(provokeTanks.ToList());
        if (armies.Count == 0) { return;  } 
        lastHealingTime = Time.time;
        foreach (GameObject army in armies)
        {
            //foreach (GameObject capsule in capsules)
            //{
                if ((transform.position - army.transform.position).sqrMagnitude < healingRange * healingRange)
                {
                    //Debug.Log($"healing {army.name} {healingAmount}");
                    cmdHealing(army, healingAmount);
                    SpecialEffect(army);
                }
            //}
        }
    }
    [Server]
    public void ServerEnableHealing(bool enabled)
    {
        HEALING_ENABLED = enabled;
    }
    [Command]
    public void cmdHealing(GameObject unit , int amount)
    {
        unit.GetComponent<Health>().Healing(healingAmount);
    }
    public void SpecialEffect(GameObject army)
    {
        if (isServer)
            RpcHealingPrefab(army);
        else
            cmdHealingPrefab(army);
    }
    [Command]
    public void cmdHealingPrefab(GameObject army)
    {
        ServerHealingPrefab(army);
    }
    [Server]
    public void ServerHealingPrefab(GameObject army)
    {
        HandleHealingPrefab(army);
    }
    [ClientRpc]
    public void RpcHealingPrefab(GameObject army)
    {
        HandleHealingPrefab(army);
    }
    public void HandleHealingPrefab(GameObject army)
    {
        //Debug.Log($"HandleHealingPrefab {army.GetComponent<Targeter>().GetAimAtPoint() }");
        GameObject fxEffect = Instantiate(healingPrefab, army.GetComponent<Targeter>().GetAimAtPoint() );
        NetworkServer.Spawn(fxEffect, connectionToClient);
    }
}
