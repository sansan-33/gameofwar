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
    public void CmdUnitPowerUp()
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
                    ServerPowerUp(unit.gameObject,2,1,0,0,0,-1,0,0);
                    Scale(unitTransform, unit.gameObject);
                    RpcScale(unitTransform, unit.gameObject);
                    break;
                case UnitMeta.UnitType.CAVALRY :
                    ServerSetSpeed(10,false);
                    break;
            }
        }
    }
    
   //[Command(requiresAuthority = false)]
    [Command(ignoreAuthority = true)]
    public void CmdPowerUp(GameObject unit, int star, int cardLevel, int health, int attack, float repeatAttackDelay, int speed, int defense, int special)
    {
        //Debug.Log($"CmdPowerUp Speed ==  > {speed}");
        ServerPowerUp(unit.gameObject, star, cardLevel, health, attack, repeatAttackDelay, speed, defense, special);
    }
    [Command]
    public void cmdSpeedUp(int speed)
    {
        //Debug.Log($"cmd speed up ? {speed}");
        ServerSetSpeed(speed,true);
    }
    [Server]
    public void ServerPowerUp(GameObject unit, int star, int cardLevel, int health, int attack, float repeatAttackDelay, int speed, int defense, int special)
    {
        //Debug.Log("ServerpowerUp");
        RpcPowerUp(unit.gameObject, star, cardLevel, health, attack, repeatAttackDelay, speed, defense, special);
    }
    public void powerUp(GameObject unit, int star,int cardLevel, int health, int attack, float repeatAttackDelay, int speed, int defense, int special)
    {
        //Debug.Log($"{unit.tag} : {unit.name} ==> powerUp , star {star} ,cardLevel {cardLevel}, health {health}, attack {attack}, repeatAttackDelay {repeatAttackDelay}, speed {speed}, defense {defense}, special {special} ");
        SetSpeed(speed,false);
        unit.GetComponent<CardStats>().SetCardStats(star, cardLevel, health, attack, repeatAttackDelay,  speed,defense, special );
        unit.GetComponent<HealthDisplay>().SetUnitLevel(cardLevel, unit.GetComponent<Unit>().unitType );
        unit.GetComponent<Health>().ScaleMaxHealth(health, star);
        unit.GetComponent<IAttack>().ScaleDamageDeal(attack, repeatAttackDelay, (star == 1) ? star : (star - 1) * 3);
        unit.GetComponentInChildren<UnitBody>().SetRenderMaterial(unit, NetworkClient.connection.identity.GetComponent<RTSPlayer>().GetPlayerID(),star);
    }
    [ClientRpc]
    public void RpcPowerUp(GameObject unit, int star, int cardLevel, int health, int attack, float repeatAttackDelay, int speed, int defense, int special)
    {
        //Debug.Log("RpcPowerUp");
        powerUp(unit, star, cardLevel, health, attack, repeatAttackDelay, speed, defense, special);
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
    public void ServerSetSpeed(int speed, bool accumulate)
    {
        SetSpeed(speed, accumulate);
    }
    public void SetSpeed(int speed, bool accumulate)
    {
        if (speed < 0) { return; }
        if (agent.speed < GetComponent<UnitMovement>().maxSpeed)
        {
            //SpeedUp(agent, speed);
            RpcSpeedUp(agent.transform.gameObject, speed, accumulate);
        }
        /*
        if (canSpawnEffect)
        {
            GameObject specialEffect = Instantiate(specialEffectPrefab, GetComponentInParent<Transform>());
            NetworkServer.Spawn(specialEffect, connectionToClient);
            canSpawnEffect = false;
        }
        */
    }
    private void SpeedUp(NavMeshAgent agent, int speed, bool accumulate)
    {
        if (accumulate && agent.speed < 3  ) { return; }
        agent.speed = accumulate ? agent.speed + speed : speed;
    }
    [ClientRpc]
    private void RpcSpeedUp(GameObject agent, int speed, bool accumulate)
    {
        SpeedUp(agent.GetComponent<UnitMovement>().GetNavMeshAgent() , speed, accumulate);
    }

}
