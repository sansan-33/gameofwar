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
    private bool SPEARMANCanPowerUp = true;
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
            if (unit.unitType == UnitMeta.UnitType.SPEARMAN&& SPEARMANCanPowerUp)
            {
                ServerPowerUp(unit.gameObject, 2);
                
                Scale(unitTransform, unit.gameObject);
                RpcScale(unitTransform, unit.gameObject);
                SPEARMANCanPowerUp = false;
            }
            else if (unit.unitType == UnitMeta.UnitType.KNIGHT)
            {
                ServerSetSpeed();
            }
        }
       
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
            ResetSpeed(agent);
            RpcResetSpeed(agent.transform.gameObject);
        }
        if (canSpawnEffect)
        {
            GameObject specialEffect = Instantiate(specialEffectPrefab, GetComponentInParent<Transform>());
            NetworkServer.Spawn(specialEffect, connectionToClient);
            canSpawnEffect = false;
        }
    }
    private void ResetSpeed(NavMeshAgent agent)
    {
        agent.speed += 10;
       
    }
    [ClientRpc]
    private void RpcResetSpeed(GameObject agent)
    {
        ResetSpeed(agent.GetComponent<UnitMovement>().GetNavMeshAgent());
    }

}
