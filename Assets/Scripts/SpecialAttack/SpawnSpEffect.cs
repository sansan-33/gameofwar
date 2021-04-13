using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class SpawnSpEffect : NetworkBehaviour
{
    [SerializeField] private GameObject[] effectList = new GameObject[0];
    // Start is called before the first frame update
    [Command(ignoreAuthority = true)]
    public void CmdSpawnEffect(int effectNum, Transform transform)
    {
        //Debug.Log($"{effectNum},{transform} at cmd");
        ServerSetShield(effectNum, transform);
    }
    [Server]
    public void ServerSetShield(int effectNum, Transform transform)
    {
        //Debug.Log($"{effectNum},{transform} at server");
        RpcSetShield(effectNum, transform);
    }
    [ClientRpc]
    public void RpcSetShield(int effectNum, Transform transform)
    {
        //Debug.Log($"{effectNum},{transform} at rpc");
        SetShield(effectNum, transform);
    }
    public void SetShield(int effectNum, Transform transform)
    {
        //Debug.Log($"{effectNum},{transform} at final");
        Instantiate(effectList[effectNum], transform);
    }
    // Update is called once per frame
    
}
