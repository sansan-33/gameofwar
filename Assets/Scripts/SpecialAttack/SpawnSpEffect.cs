using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class SpawnSpEffect : NetworkBehaviour
{
    [SerializeField] private GameObject[] effectList = new GameObject[0];
    private List<GameObject> effectLists1 = new List<GameObject>();
    private List<GameObject> effectLists2 = new List<GameObject>();
    private List<GameObject> effectLists3 = new List<GameObject>();
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
        GameObject effect;
        //Debug.Log($"{effectNum},{transform} at final");
        Debug.Log($"transform { transform.name} tag {transform.tag}");
        if (transform == null)
        {
            effect = Instantiate(effectList[effectNum]);
        }
         effect = Instantiate(effectList[effectNum], transform);
        Debug.Log($"effect { effect.transform.parent.name} tag {effect.transform.parent.tag}");
        switch (effectNum)
        {
            case 0:
                effectLists1.Add(effect);
                break;
            case 1:
                effectLists2.Add(effect);
                break;
            case 2:
                effectLists3.Add(effect);
                break;
        }
    }

    public List<GameObject> GetEffect(int i)
    {
        switch (i)
        {
            case 0:
                return effectLists1;
                break;
            case 1:
                return effectLists2;
                break;
            default:
                return effectLists3;
                break;
        }
    }

    // Update is called once per frame
    
}
