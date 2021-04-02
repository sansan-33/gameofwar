using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class DefendSP : MonoBehaviour
{
    private SpCost spCost;

    public float SPCost = 10;
    public int shieldHealths = 100;
    private Button SPButton;
    private RTSPlayer player;
    void Start()
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        spCost = FindObjectOfType<SpCost>();
        SPButton = GameObject.FindGameObjectWithTag("SpDefend").GetComponent<Button>();
        SPButton.onClick.RemoveAllListeners();
        SPButton.onClick.AddListener(OnPointerDown);
    }

    public void OnPointerDown()
    {
       
        //if(SPAmount < SPCost) {return;}
        spCost.SPAmount -= (int)SPCost;
        Unit[] shieldList;
        shieldList = FindObjectsOfType<Unit>();
        
        if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)
        {
           
            foreach (Unit shield in shieldList)
            {
               
                if (shield.CompareTag("Player0") || shield.CompareTag("King0"))
                {
                    
                    shield.GetComponent<Shield>().CmdSetShieldHealth(shieldHealths);
                    }
                }
            }
            else // Multi player seneriao
            {
                //Debug.Log($"OnPointerDown Defend SP Multi shieldList {shieldList.Length}");
                foreach (Unit shield in shieldList)
                {
                    //Debug.Log($"shield tag {shield.tag}");

                    if (shield.CompareTag("Player" + player.GetPlayerID()) || shield.CompareTag("King" + player.GetPlayerID()))
                    {
                        //shield.GetComponent<Shield>().shieldHealth = shieldHealths;
                        //Debug.Log($"player {player.GetPlayerID()} , set shield health  {shield.tag} / {shieldHealths}");

                        shield.GetComponent<Shield>().CmdSetShieldHealth(shieldHealths);
                        //CommandShield(shield, shieldHealths);
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
