using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class UnitPowerUp : NetworkBehaviour
{
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
                /*
                case UnitMeta.UnitType.FOOTMAN :
                    if (unit.isScaled) { break; }
                    ServerPowerUp(unit.gameObject,2,1,0,0,0,-1,0,0);
                    Scale(unitTransform, unit.gameObject);
                    RpcScale(unitTransform, unit.gameObject);
                    break;
                */
                case UnitMeta.UnitType.CAVALRY :
                    ServerSetSpeed(10,false);
                    break;
            }
        }
    }
    
   //[Command(requiresAuthority = false)]
    [Command(ignoreAuthority = true)]
    public void CmdPowerUp(GameObject unit, int star, int cardLevel, int health, int attack, float repeatAttackDelay, int speed, int defense, int special, string specialkey, string passivekey)
    {
        //Debug.Log($"CmdPowerUp Speed ==  > {speed}");
        ServerPowerUp(unit.gameObject, star, cardLevel, health, attack, repeatAttackDelay, speed, defense, special, specialkey, passivekey);
    }
    [Command]
    public void cmdSpeedUp(int speed)
    {
        //Debug.Log($"cmd speed up ? {speed}");
        ServerSetSpeed(speed,true);
    }
    [Server]
    public void ServerPowerUp(GameObject unit, int star, int cardLevel, int health, int attack, float repeatAttackDelay, int speed, int defense, int special, string specialkey, string passivekey)
    {
        //Debug.Log("ServerpowerUp");
        powerUp(unit, star, cardLevel, health, attack, repeatAttackDelay, speed, defense, special, specialkey, passivekey);
        RpcPowerUp(unit.gameObject, star, cardLevel, health, attack, repeatAttackDelay, speed, defense, special, specialkey, passivekey);
    }
    public void powerUp(GameObject unit, int star,int cardLevel, int health, int attack, float repeatAttackDelay, int speed, int defense, int special, string specialkey, string passivekey)
    {
        //Debug.Log($"{unit.tag} : {unit.name} ==> powerUp , star {star} ,cardLevel {cardLevel}, health {health}, attack {attack}, repeatAttackDelay {repeatAttackDelay}, speed {speed}, defense {defense}, special {special} ");
        SetSpeed(speed,false);
        unit.GetComponent<CardStats>().SetCardStats(star, cardLevel, health, attack, repeatAttackDelay,  speed,defense, special, specialkey, passivekey);
        unit.GetComponent<HealthDisplay>().SetUnitLevel(cardLevel, unit.GetComponent<Unit>().unitType );
        unit.GetComponent<Health>().ScaleMaxHealth(health, star);
        unit.GetComponent<IAttack>().ScaleDamageDeal(attack, repeatAttackDelay, (star == 1) ? star : (star - 1) * 3);
        unit.GetComponentInChildren<UnitBody>().SetRenderMaterial(unit, NetworkClient.connection.identity.GetComponent<RTSPlayer>().GetPlayerID(),star);
    }
    [ClientRpc]
    public void RpcPowerUp(GameObject unit, int star, int cardLevel, int health, int attack, float repeatAttackDelay, int speed, int defense, int special, string specialkey, string passivekey)
    {
        Debug.Log($"{unit.tag} : {unit.name} RpcPowerUp cardLevel {cardLevel} health {health} speed {speed}");
        powerUp(unit, star, cardLevel, health, attack, repeatAttackDelay, speed, defense, special, specialkey, passivekey);
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
        RpcSpeedUp(speed, accumulate);
    }
    public void SetSpeed(int speed, bool accumulate)
    {
        if (speed < 0) { return; }
        if (GetComponent<Unit>().GetUnitMovement().GetSpeed(UnitMeta.SpeedType.CURRENT)   < GetComponent<Unit>().GetUnitMovement().GetSpeed(UnitMeta.SpeedType.MAX)  )
        {
            SpeedUp(speed, accumulate);
            //RpcSpeedUp(speed, accumulate);
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
    private void SpeedUp(int speed, bool accumulate)
    {
        float currentSpeed = GetComponent<Unit>().GetUnitMovement().GetSpeed(UnitMeta.SpeedType.CURRENT);
        if (accumulate && currentSpeed < 1  ) { return; }
        GetComponent<Unit>().GetUnitMovement().SetSpeed(UnitMeta.SpeedType.CURRENT, accumulate ? currentSpeed + speed : speed);
    }
    [ClientRpc]
    private void RpcSpeedUp( int speed, bool accumulate)
    {
        SpeedUp( speed, accumulate);
    }
    [Command]
    public void CmdTag(GameObject unit, int playerID, string unitName, Color teamColor, int spawnPointIndex, int star, int cardLevel, int health, int attack, float repeatAttackDelay, int speed, int defense, int special, string specialkey, string passivekey)
    {
        ServerTag(unit, playerID, unitName, teamColor, spawnPointIndex, star, cardLevel, health, attack, repeatAttackDelay, speed, defense, special, specialkey, passivekey);
    }
    [Server]
    public void ServerTag(GameObject unit, int playerID, string unitName, Color teamColor, int spawnPointIndex, int star, int cardLevel, int health, int attack, float repeatAttackDelay, int speed, int defense, int special, string specialkey, string passivekey)
    {
        RpcTag(unit, playerID, unitName, teamColor, spawnPointIndex, star, cardLevel, health, attack, repeatAttackDelay, speed, defense, special, specialkey, passivekey);
    }
    [ClientRpc]
    void RpcTag(GameObject unit, int playerID, string unitName, Color teamColor, int spawnPointIndex, int star, int cardLevel, int health, int attack, float repeatAttackDelay, int speed, int defense, int special, string specialkey, string passivekey)
    {
        Debug.Log($"Rpc Tag {playerID} {unit.name}");
        unit.name = unitName;
        unit.tag = ((unit.GetComponent<Unit>().unitType == UnitMeta.UnitType.KING) ? "King" : "Player") + playerID;
        unit.GetComponent<HealthDisplay>().SetHealthBarColor(teamColor);
        unit.GetComponentInChildren<UnitBody>().ServerChangeUnitRenderer(unit, playerID, star);
        unit.GetComponent<Unit>().SetSpawnPointIndex(spawnPointIndex);
        unit.GetComponent<CardStats>().SetCardStats(star, cardLevel, health, attack, repeatAttackDelay, speed, defense, special, specialkey, passivekey);
        //unit.GetComponent<HealthDisplay>().SetUnitLevel(cardLevel, unit.GetComponent<Unit>().unitType);
        //unit.GetComponent<Health>().ScaleMaxHealth(health, star);
    }
}
