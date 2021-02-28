using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class BattleFieldRules : MonoBehaviour
{
    public  GameObject MiddleLine;
    private Camera mainCamera;
    RTSPlayer player;
    void Start()
    {
        MiddleLine = GameObject.FindGameObjectWithTag("MiddleLine");
           player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        mainCamera = Camera.main;
    }
    public bool IsInField(Transform unit)
    {
        //return false;


        if (unit.GetComponent<NetworkIdentity>().hasAuthority)
        {
            if (MiddleLine.transform.position.z > unit.position.z)
            {
                Debug.Log($"PlayerID == 0 -- true{player.GetPlayerID()}");
                //  Debug.Log($"IsInFields-->{IsInFields}");
                return true;
            }
            else
            {
                Debug.Log($"PlayerID == 0 -- false{player.GetPlayerID()}");
                //Debug.Log($"IsNotInFields-->{IsNotInField}");
                return false;
            }
        }
        else
        {
            if (MiddleLine.transform.position.z < unit.position.z)
            {
                Debug.Log($"PlayerID != 0 -- true{player.GetPlayerID()}");
                //  Debug.Log($"IsInFields-->{IsInFields}");
                return true;
            }
            else
            {
                Debug.Log($"PlayerID != 0 -- false{player.GetPlayerID()}");
                //Debug.Log($"IsNotInFields-->{IsNotInField}");
                return false;
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
