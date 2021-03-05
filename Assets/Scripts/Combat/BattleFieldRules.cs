using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class BattleFieldRules : MonoBehaviour
{
    public  GameObject MiddleLine;
    public Transform unitTransform;
    void Start()
    {
        MiddleLine = GameObject.FindGameObjectWithTag("MiddleLine");
    }
    public bool IsInField()
    {
        return true;

       /* if (unitTransform.GetComponent<NetworkIdentity>().hasAuthority)
        {
            if (MiddleLine.transform.position.z > unitTransform.position.z)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            if (MiddleLine.transform.position.z < unitTransform.position.z)
            {
                return true;
            }
            else
            {
                return false;
            }
        }*/
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
