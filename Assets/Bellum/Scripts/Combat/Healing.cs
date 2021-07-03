using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Healing : NetworkBehaviour
{
    private int healingRange = 25;
    private int repeatHealingDelay = 2;
    private float lastHealingTime;
    private int healingAmount = 2;
    [SerializeField] GameObject healingPrefab;
    List<GameObject> armies = new List<GameObject>();
    [SyncVar]
    public bool particleSysytemPlay = false;
    [SyncVar]
    private bool HEALING_ENABLED = false;
    private ParticleSystem healingPS;

    // Healing Army reference
    RTSPlayer rTSPlayer;
    // Start is called before the first frame update
    void Start()
    {
        GameObject healingObj = Instantiate(healingPrefab, GetComponent<Targeter>().GetAimAtPoint());
        healingPS = healingObj.GetComponent<ParticleSystem>();
        lastHealingTime = Time.time + 5f;
        rTSPlayer = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
    }
    // Update is called once per frame

    private void Update()
    {
        if (lastHealingTime + repeatHealingDelay > Time.time) { return; }

        playParticle();
        handleHealing();
    }
    private void playParticle()
    {
        if (particleSysytemPlay)
        {
            healingPS.Play();
            particleSysytemPlay = false;
        }
        else
        {
            healingPS.Stop();
        }
    }
    private void handleHealing()
    {
        if (!HEALING_ENABLED) { return; }

        //if (this.isLocalPlayer) { return; }

        //if (!hasAuthority) { return; }

        String playerid = tag.Substring(tag.Length - 1);
        //if(king == null)
        //    king = GameObject.FindGameObjectWithTag("King" + playerid);

        //GameObject[] units = GameObject.FindGameObjectsWithTag("Player" + playerid);
        //GameObject[] provokeTanks = GameObject.FindGameObjectsWithTag("Provoke" + playerid);
        //armies = new List<GameObject>();
        //if (units != null && units.Length > 0)
        //    armies = units.ToList();
        //if (king != null)
        //    armies.Add(king);
        //if (provokeTanks != null && provokeTanks.Length > 0)
        //    armies.AddRange(provokeTanks.ToList());
        //if (armies.Count == 0) { return; }
        lastHealingTime = Time.time;
        foreach (Unit unit in rTSPlayer.GetMyUnits()) {
            //Debug.Log($"Healing {name} : {unit.unitType} , tag {tag}");
            if (unit.tag.Substring(unit.tag.Length - 1) != playerid) { continue; }
            if ((transform.position - unit.transform.position).sqrMagnitude < healingRange * healingRange)
            {
                cmdHealing(unit.gameObject, healingAmount);
                unit.GetComponent<Healing>().particleSysytemPlay = true;
            }
        }
        /*
        foreach (GameObject army in armies)
        {
            if ((transform.position - army.transform.position).sqrMagnitude < healingRange * healingRange)
            {
                //Debug.Log($"healing {army.name} {healingAmount}");
                cmdHealing(army, healingAmount);
                army.GetComponent<Healing>().particleSysytemPlay = true;
            }
        }
        */
    }
    public void enableHealing(bool enabled)
    {
        HEALING_ENABLED = enabled;
    }
    [Command]
    public void cmdHealing(GameObject unit , int amount)
    {
        unit.GetComponent<Health>().Healing(healingAmount);
    }
    public void HandleHealingPrefab(GameObject army)
    {
        //Debug.Log($"HandleHealingPrefab {army.GetComponent<Targeter>().GetAimAtPoint() }");
        if (army == null || !army.GetComponent<Health>().IsAlive()) { return; }
        army.GetComponent<Healing>().particleSysytemPlay = true;
    }
}
