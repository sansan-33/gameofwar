using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using static SpecialAttackDict;
public class SpecialAttackManager : NetworkBehaviour
{
    private RTSPlayer RTSplayer;
    // Start is called before the first frame update
    private Dictionary<string, SpecialAttackType> SpecialAttackTypeStringKey = new Dictionary<string, SpecialAttackType>()
    {
        { "FIREARROW",SpecialAttackType.FIREARROW },
        { "FREEZE",SpecialAttackType.FREEZE },
       { "METEOR",SpecialAttackType.METEOR },
       { "TORNADO",SpecialAttackType.TORNADO },
       { "ZAP",SpecialAttackType.ZAP },
       { "LIGHTNING",SpecialAttackType.LIGHTNING },
    };
    void Start()
    {
        RTSplayer = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
    }
    public void SpawnPrefab(Vector3 pos, string specialAttackType)
    {
        if (isServer)
            RpcSpawnPrefab(pos, specialAttackType);
        else
            CmdSpawnPrefab(pos, specialAttackType);
    }
    //[Command(ignoreAuthority = true)]
    [Command(requiresAuthority = false)]
    public void CmdSpawnPrefab(Vector3 pos, string specialAttackType)
    {
        //Debug.Log($"CmdPowerUp {pos} {specialAttackType } ");
        ServerSpawnPrefab(pos, specialAttackType);
    }
    [Server]
    public void ServerSpawnPrefab(Vector3 pos, string specialAttackType)
    {
        //Debug.Log($"ServerpowerUp {pos} {specialAttackType } ");
        HandleSpawnPrefab(pos, specialAttackType);
        //if comment this line . player 2 name is lower case with (clone) , card stats and other things not sync
        RpcSpawnPrefab(pos, specialAttackType);
    }
    public void HandleSpawnPrefab( Vector3 pos, string specialAttackType)
    {
        //Debug.Log($"HandleSpawnPrefab 1 {specialAttackType}");
        SpecialAttackTypeStringKey.TryGetValue(specialAttackType, out SpecialAttackType SpecialAttackType);
        //Debug.Log($"HandleSpawnPrefab 2 {specialAttackType}, {SpecialAttackType}");
        GetComponent<SpButtonManager>().SpecialAttackPrefab.TryGetValue(SpecialAttackType, out GameObject impectPrefab);
        //Debug.Log($"HandleSpawnPrefab 3{impectPrefab}, {SpecialAttackType}");
        GameObject impect = null;
        if (impectPrefab != null)
        {
           
            impect = Instantiate(impectPrefab);
            impect.transform.position = pos;
        }
        if (SpecialAttackType == SpecialAttackDict.SpecialAttackType.TORNADO)
        {

            impect.GetComponent<Tornado>().SetPlayerType(RTSplayer.GetPlayerID());
            impect.GetComponent<Tornado>().OnStartServer();
            //StartCoroutine(DestroyGameObjectAfterSec(impect, 5.5f));
        }
    }
    [ClientRpc]
    public void RpcSpawnPrefab(Vector3 pos, string specialAttackType)
    {
        //Debug.Log($"RpcPowerUp {pos} {specialAttackType }  ");
        HandleSpawnPrefab( pos, specialAttackType);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
