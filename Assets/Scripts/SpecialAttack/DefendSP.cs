using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class DefendSP : MonoBehaviour
{
    private List<GameObject> enemyList;
    private Button SPButton;
    private RTSPlayer player;
    private SpCost spCost;

    public int SPCost = 10;
    public int shieldHealths = 10000;
    public int buttonTicket;
    private bool SpawnedButton;
    void Start()
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        if (CompareTag("King" + player.GetEnemyID())|| CompareTag("Player" + player.GetEnemyID())) { return; }
        spCost = FindObjectOfType<SpCost>();
        //Instantiate SpButton and is it already spawned
        SpawnedButton = FindObjectOfType<SpButton>().InstantiateSpButton(SpecialAttackDict.SpecialAttackType.Shield, GetComponent<Unit>());
        //SpButton will give unit a int to get back the button that it spwaned
        if (SpawnedButton) { SPButton = FindObjectOfType<SpButton>().GetButton(GetComponent<Unit>().SpBtnTicket).GetComponent<Button>(); }
        if(SPButton == null) { return; }
        SPButton.onClick.RemoveAllListeners();
        SPButton.onClick.AddListener(OnPointerDown);
    }

    public void OnPointerDown()
    {

        if (spCost.SPAmount < SPCost) { return; }
        spCost.UpdateSPAmount(-SPCost);
        Unit[] shieldList;
        //find all unit
        shieldList = FindObjectsOfType<Unit>();
        enemyList = GameObject.FindGameObjectsWithTag("Player" + player.GetEnemyID()).ToList();

        if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)//1 player mode
        {
            foreach (Unit shield in shieldList)
            {  // Only Set on our side
                if (shield.CompareTag("Player0") || shield.CompareTag("King0"))
                {
                    // Set shield health
                    shield.GetComponent<Shield>().CmdSetShieldHealth(shieldHealths);

                }
            }

        }
        else // Multi player seneriao
        {
            //Debug.Log($"OnPointerDown Defend SP Multi shieldList {shieldList.Length}");
            foreach (Unit shield in shieldList)
            {  // Only Set on our side
                if (shield.CompareTag("Player" + player.GetPlayerID()) || shield.CompareTag("King" + player.GetPlayerID()))
                {
                    // Set shield health
                    shield.GetComponent<Shield>().CmdSetShieldHealth(shieldHealths);
                }
            }

        }
        /*
            if (player.GetPlayerID() == 0)
            {
              foreach (GameObject shield in shieldList)
              {
                if (shield.transform.parent.CompareTag("Player0") || shield.transform.parent.CompareTag("King0"))
                {

                    shield.GetComponent<Shield>().shieldHealth = shieldHealths;
                    CommandShield(shield, shieldHealths);
                }
               }
            }
            else
            {
              foreach (GameObject shield in shieldList)
              {
                if (shield.transform.parent.CompareTag("Player1") || shield.transform.parent.CompareTag("King1"))
                {

                    shield.GetComponent<Shield>().shieldHealth = shieldHealths;

                    CommandShield(shield, shieldHealths);
                }
              }
            }
        */


    }
    /*
    [Command]
    void CommandShield(GameObject shield, int shieldHealths)
    {
        ServerShield(shield, shieldHealths);
    }
    [Server]
    void ServerShield(GameObject shield, int shieldHealths)
    {
        shield.GetComponent<Shield>().shieldHealth = shieldHealths;
    }
    [ClientRpc]
    void RpcShield(GameObject shield,int shieldHealths)
    {       
       
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    */
}
