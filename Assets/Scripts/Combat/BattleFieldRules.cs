using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class BattleFieldRules : MonoBehaviour
{
    private Camera mainCamera;
    RTSPlayer player;
    bool IsInFields = true;
    bool IsNotInField = false;
    void Start()
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        mainCamera = Camera.main;
    }
    public bool IsInField(Transform unit)
    {
        return false;
        /*
       
        if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)
        {
            IsInFields = true;
            IsNotInField = false;
        }
        else if  (player.GetPlayerID() == 1)
        {
            IsInFields = false;
            IsNotInField = true;
        }
        else
        {
            IsInFields = true;
            IsNotInField = false;
        }
        if (mainCamera.WorldToScreenPoint(unit.position).y < (Screen.height / 2) - 200)
        {
          //  Debug.Log($"IsInFields-->{IsInFields}");
            return IsInFields;
        }
        else
        {
            //Debug.Log($"IsNotInFields-->{IsNotInField}");
            return IsNotInField;
        }
        */
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
