using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class UnitPowerUp : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] private GameObject specialEffectPrefab = null;
    [SerializeField] private BattleFieldRules battleFieldRules = null;
    public bool canSpawnEffect = true;
    public bool CanHalfSpeed = true;
    public bool CanTimeSpeed = true;
    [Command]
    public void cmdPowerUp()
    {
        Unit unit = GetComponentInParent<Unit>();
        Transform unitTransform = GetComponentInParent<Transform>();
        
        bool CanPowerUp = false;
        if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)
        {
            if (CompareTag("Player0"))
            {
                CanPowerUp = true;
            }
        }else {
            CanPowerUp = true;
        }
        
        if (!battleFieldRules.IsInField() && CanPowerUp)
        {
            switch (unit.unitType)
            {
                case UnitMeta.UnitType.FOOTMAN :
                    if (unit.isScaled) { break; }
                    Debug.Log("FOOTMAN Scale Up");
                    ServerPowerUp(unit.gameObject, 2);
                    Scale(unitTransform, unit.gameObject);
                    RpcScale(unitTransform, unit.gameObject);
                    break;
                case UnitMeta.UnitType.CAVALRY :
                    ServerSetSpeed();
                    break;
            }
        }
       /*
        if (battleFieldRules.IsInField() && CanHalfSpeed)
        {
            agent.speed /= 2;
            CanHalfSpeed = false;
            CanTimeSpeed = true;
        }
        else if(!battleFieldRules.IsInField() && CanTimeSpeed)
        {
            agent.speed *= 2;
            CanTimeSpeed = false;
            CanHalfSpeed = true;
        }
       */
    }
    [Server]
    public void ServerPowerUp(GameObject unit, int star)
    {
        RpcPowerUp(unit.gameObject, star);
    }
    public void powerUp(GameObject unit, int star)
    {
        //Debug.Log(unit);
        unit.GetComponent<Health>().ScaleMaxHealth(star);

        if (star == 1)
        {
            unit.GetComponent<IAttack>().ScaleDamageDeal(star);
        }
        else
        {
            unit.GetComponent<IAttack>().ScaleDamageDeal((star - 1) * 3);
        }
        //Debug.Log("powerUp");
      
        unit.GetComponentInChildren<UnitBody>().SetRenderMaterial(unit, NetworkClient.connection.identity.GetComponent<RTSPlayer>().GetPlayerID(),star);
        //unit.GetComponentInChildren<IBody>().SetUnitSize(star);

        //return unit.GetComponent<Unit>();
    }
    [ClientRpc]
    public void RpcPowerUp(GameObject unit, int star)
    {
        //Debug.Log("RpcPowerUp");
        powerUp(unit, star);
    }
    private void Scale(Transform unitTransform, GameObject unit)
    {
        unitTransform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        unit.GetComponent<IAttack>().ScaleAttackRange(1.2f) ;
        unit.GetComponent<Unit>().isScaled = true;
    }
    [ClientRpc]
    private void RpcScale(Transform unitTransform, GameObject unit)
    {
        Scale(unitTransform, unit);
    }
    [Server]
    public void ServerSetSpeed()
    {
        if (agent.speed < GetComponent<UnitMovement>().maxSpeed)
        {
            SpeedUp(agent);
            RpcSpeedUp(agent.transform.gameObject);
        }
        if (canSpawnEffect)
        {
            GameObject specialEffect = Instantiate(specialEffectPrefab, GetComponentInParent<Transform>());
            NetworkServer.Spawn(specialEffect, connectionToClient);
            canSpawnEffect = false;
        }
    }
    private void SpeedUp(NavMeshAgent agent)
    {
        agent.speed += 10;
    }
    [ClientRpc]
    private void RpcSpeedUp(GameObject agent)
    {
        SpeedUp(agent.GetComponent<UnitMovement>().GetNavMeshAgent());
    }

}
