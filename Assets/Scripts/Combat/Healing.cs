using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Healing : NetworkBehaviour
{
    private TacticalBehavior tb;
    private RTSPlayer player;
    private GameObject[] capsules;
    private int healingRange = 5;
    private int repeatHealingDelay = 1;
    private float lastHealingTime;
    private int healingAmount = 1;

    // Start is called before the first frame update
    void Start()
    {
        tb = FindObjectOfType<TacticalBehavior>();
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        lastHealingTime = Time.time + 5f;
    }
    // Update is called once per frame
   
    private void Update()
    {
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

        GameObject[] armies = GameObject.FindGameObjectsWithTag("Player" + player.GetPlayerID());
        
        lastHealingTime = Time.time;
        foreach (GameObject army in armies)
        {
            foreach (GameObject capsule in capsules)
            {
                if ((capsule.transform.position - army.transform.position).sqrMagnitude < healingRange * healingRange)
                {
                    cmdHealing(army, healingAmount);
                    cmdHealingPrefab(capsule, true);
                }
            }
        }*/
    }
    [Command]
    public void cmdHealing(GameObject unit , int amount)
    {
        unit.GetComponent<Health>().Healing(healingAmount);
    }
    [Command]
    public void cmdHealingPrefab(GameObject capsule, bool isActive)
    {
        RpcHealingPrefab(capsule, isActive);
    }
    [ClientRpc]
    public void RpcHealingPrefab(GameObject capsule, bool isActive)
    {
        //Debug.Log($"RpcHealingPrefab {isActive}");
        capsule.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = isActive;
    }
}
