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
    //public bool CanHalfSpeed = true;
    //public bool CanTimeSpeed = true;
    bool CanPowerUp = true;
    Unit unit;
    Transform unitTransform;
    public override void OnStartAuthority()
    {
        
    }
    [Command]
    public void cmdPowerUp()
    {
        unit = GetComponentInParent<Unit>();
        unitTransform = GetComponentInParent<Transform>();
        if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1 && CompareTag("Player1")){
            CanPowerUp = false;
        }
        //Debug.Log($"cmdPowerUp CanPowerUp ? {CanPowerUp} battleFieldRules Is In Own Field() {battleFieldRules.IsInOwnField()}");
        if (!battleFieldRules.IsInOwnField() && CanPowerUp)
        {
            //Debug.Log($"cmdPowerUp {unit.unitType}");
            switch (unit.unitType)
            {
                case UnitMeta.UnitType.FOOTMAN :
                    if (unit.isScaled) { break; }
                    ServerPowerUp(unit.gameObject, 2);
                    Scale(unitTransform, unit.gameObject);
                    RpcScale(unitTransform, unit.gameObject);
                    break;
                case UnitMeta.UnitType.CAVALRY :
                    ServerSetSpeed(10);
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
    [Command]
    public void cmdSpeedUp(int speed)
    {
        Debug.Log($"cmd speed up ? {speed}");
        ServerSetSpeed(speed);
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
        //Debug.Log($"Scale Up {unit.GetComponent<Unit>().unitType}");
    }
    [ClientRpc]
    private void RpcScale(Transform unitTransform, GameObject unit)
    {
        Scale(unitTransform, unit);
    }
    [Server]
    public void ServerSetSpeed(int speed)
    {
        if (agent.speed < GetComponent<UnitMovement>().maxSpeed && agent.speed > 0)
        {
            //SpeedUp(agent, speed);
            RpcSpeedUp(agent.transform.gameObject, speed);
        }
        if (canSpawnEffect)
        {
            GameObject specialEffect = Instantiate(specialEffectPrefab, GetComponentInParent<Transform>());
            NetworkServer.Spawn(specialEffect, connectionToClient);
            canSpawnEffect = false;
        }
    }
    private void SpeedUp(NavMeshAgent agent, int speed)
    {
        if (agent.speed < 3  ) { return; }
        agent.speed += speed;
    }
    [ClientRpc]
    private void RpcSpeedUp(GameObject agent, int speed)
    {
        SpeedUp(agent.GetComponent<UnitMovement>().GetNavMeshAgent() , speed);
    }

}
