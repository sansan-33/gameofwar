using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class UnitPowerUp : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] private GameObject specialEffectPrefab = null;
    private bool SPEARMANCanPowerUp = true;
    [Command]
    public void cmdPowerUp()
    {
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

        if (!GetComponentInParent<BattleFieldRules>().IsInField(GetComponentInParent<Transform>()) && CanPowerUp)
        {
            if (GetComponentInParent<Unit>().unitType == UnitMeta.UnitType.SPEARMAN&& SPEARMANCanPowerUp)
            {
                powerUp(GetComponentInParent<Unit>(), 3);
                RpcPowerUp(GetComponentInParent<Transform>().gameObject, 3);
                Scale(GetComponentInParent<Transform>());
                RpcScale(GetComponentInParent<Transform>());
                SPEARMANCanPowerUp = false;
            }
            else if (GetComponentInParent<Unit>().unitType == UnitMeta.UnitType.KNIGHT)
            {
                ServerSetSpeed();
            }
        }
    }
    [Server]
    public Unit powerUp(Unit unit, int star)
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

        unit.GetComponentInChildren<IBody>().SetRenderMaterial(star);
        //unit.GetComponentInChildren<IBody>().SetUnitSize(star);

        return unit;
    }
    [ClientRpc]
    public void RpcPowerUp(GameObject unit, int star)
    {

        powerUp(unit.GetComponent<Unit>(), star);
    }
    private void Scale(Transform tacticalAgent)
    {
        tacticalAgent.transform.localScale = new Vector3(3, 3, 3);
    }
    [ClientRpc]
    private void RpcScale(Transform tacticalAgent)
    {
        Scale(tacticalAgent);
    }
    [Server]
    public void ServerSetSpeed()
    {
        Debug.Log($"SetSpeed {agent.speed}");
        if (agent.speed < 100)
        {
            GameObject specialEffect = Instantiate(specialEffectPrefab, GetComponentInParent<Transform>());
            ResetSpeed(agent);
            RpcResetSpeed(agent.transform.gameObject);
            NetworkServer.Spawn(specialEffect, connectionToClient);
        }
    }
    private void ResetSpeed(NavMeshAgent agent)
    {
        agent.speed = 100;
    }
    [ClientRpc]
    private void RpcResetSpeed(GameObject agent)
    {
        ResetSpeed(agent.GetComponent<UnitMovement>().GetNavMeshAgent());
    }

}
