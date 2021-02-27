using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitPowerUp : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public Unit powerUp(Unit unit, int star)
    {
        Debug.Log(unit);
        unit.GetComponent<Health>().ScaleMaxHealth(star);

        if (star == 1)
        {
            unit.GetComponent<IAttack>().ScaleDamageDeal(star);
        }
        else
        {
            unit.GetComponent<IAttack>().ScaleDamageDeal((star - 1) * 4);
        }

        unit.GetComponentInChildren<IBody>().SetRenderMaterial(star);
        //unit.GetComponentInChildren<IBody>().SetUnitSize(star);

        return unit;
    }
    [ClientRpc]
    public void RpcPowerUp(GameObject unit, int star)
    {

        powerUp(unit.GetComponent<Unit>(), star);
    }
}
