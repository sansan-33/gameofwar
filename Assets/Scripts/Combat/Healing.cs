using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Healing : MonoBehaviour
{
    private TacticalBehavior tb;
    private RTSPlayer player;
    private GameObject capsule;
    private int healingRange = 5;
    private int repeatHealingDelay = 2;
    private float lastHealingTime;
    // Start is called before the first frame update
    void Start()
    {
        tb = FindObjectOfType<TacticalBehavior>();
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        lastHealingTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {

        if (tb.GetBehaviorSelectionType() != TacticalBehavior.BehaviorSelectionType.Defend) { return; }

        if (lastHealingTime + repeatHealingDelay > Time.time) { return;  }

        GameObject[] armies = GameObject.FindGameObjectsWithTag("Player" + player.GetPlayerID());
        capsule = GameObject.FindGameObjectWithTag("PlayerBase" + player.GetPlayerID());
        lastHealingTime = Time.time;
        foreach (GameObject army in armies)
        {
            if ((capsule.transform.position - army.transform.position).sqrMagnitude < healingRange * healingRange)
                army.GetComponent<Health>().Healing(Health.healingSpeed);

        }
        
    }
}
