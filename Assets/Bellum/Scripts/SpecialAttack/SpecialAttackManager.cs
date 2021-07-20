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
        { "STUN",SpecialAttackType.STUN },
        { "REMOVEGAUGE",SpecialAttackType.REMOVEGAUGE },
        { "GRAB",SpecialAttackType.GRAB },
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
    public void SpawnGrabPrefab(Vector3 toPosVector3 , Vector3 pos, string specialAttackType, GameObject unit)
    {
        if (isServer)
            RpcSpawnGrabPrefab(toPosVector3, pos, specialAttackType, unit);
        else
            CmdSpawnGrabPrefab(toPosVector3, pos, specialAttackType, unit);
    }
    //[Command(ignoreAuthority = true)]
    [Command(requiresAuthority = false)]
    public void CmdSpawnGrabPrefab(Vector3 toPos , Vector3 pos, string specialAttackType, GameObject unit)
    {
        //Debug.Log($"CmdPowerUp {pos} {specialAttackType } ");
        ServerSpawnGrabPrefab(toPos, pos, specialAttackType, unit);
    }
    [Server]
    public void ServerSpawnGrabPrefab(Vector3 toPosVector3, Vector3 pos, string specialAttackType, GameObject unit)
    {

        //Debug.Log($"ServerpowerUp {pos} {specialAttackType } ");
        StartCoroutine(HandleSpawnGrabPrefab(toPosVector3, pos, specialAttackType, unit));
        //if comment this line . player 2 name is lower case with (clone) , card stats and other things not sync
        RpcSpawnGrabPrefab(toPosVector3, pos, specialAttackType, unit);
    }
    public IEnumerator HandleSpawnGrabPrefab(Vector3 toPosVector3, Vector3 backPos, string specialAttackType, GameObject unit)
    {
        if (unit != null)
        {
            Vector3 currentVelocity = Vector3.zero;
            //Debug.Log($"HandleSpawnPrefab 1 {specialAttackType}");
            SpecialAttackTypeStringKey.TryGetValue(specialAttackType, out SpecialAttackType SpecialAttackType);
            //Debug.Log($"HandleSpawnPrefab 2 {specialAttackType}, {SpecialAttackType}");
            GetComponent<SpButtonManager>().SpecialAttackPrefab.TryGetValue(SpecialAttackType, out GameObject impectPrefab);
            //Debug.Log($"HandleSpawnPrefab 3{impectPrefab}, {SpecialAttackType}");
            GameObject impect = null;
            if (impectPrefab != null)
            {
                impect = Instantiate(impectPrefab);
                impect.transform.position = backPos;
            }
            Debug.Log("grab prefab start");
            float timer = 1;
            while (impect.transform.position != unit.transform.position)
            {
                timer -= Time.deltaTime;
                Debug.Log($"grab prefab start {impect.transform.position}");
                impect.transform.position = Vector3.SmoothDamp(impect.transform.position, unit.transform.position, ref currentVelocity, 0.5f);
                yield return new WaitForSeconds(0.01f);
                if (timer <= 0) { Debug.Log("Time break"); break; }
            }
            timer = 1;
            Debug.Log("unit back");
            StartCoroutine(moveUnit(unit, backPos));
            Debug.Log("grab prefab");
            while (impect.transform.position != backPos)
            {
                timer -= Time.deltaTime;
                Debug.Log($"grab prefab back {impect.transform.position}");
                impect.transform.position = Vector3.SmoothDamp(impect.transform.position, backPos, ref currentVelocity, 0.5f);
                yield return new WaitForSeconds(0.01f);
                if (timer <= 0) { Debug.Log("Time break"); break; }
            }
        }
        


    }
    private IEnumerator moveUnit(GameObject unit, Vector3 pos)
    {
        float timer = 1;
        Vector3 currentVelocity = Vector3.zero;
        while (unit.transform.position != pos)
        {
            Debug.Log($"unit prefab back {unit.transform.position}");
            unit.transform.position = Vector3.SmoothDamp(unit.transform.position, pos, ref currentVelocity, 0.5f);
            yield return new WaitForSeconds(0.01f);
            if (timer <= 0) { Debug.Log("Time break"); break; }
        }
    }
    [ClientRpc]
    public void RpcSpawnGrabPrefab(Vector3 toPosVector3 , Vector3 pos, string specialAttackType, GameObject unit)
    {
        //Debug.Log($"RpcPowerUp {pos} {specialAttackType }  ");
        StartCoroutine(HandleSpawnGrabPrefab(toPosVector3,pos, specialAttackType, unit));
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
