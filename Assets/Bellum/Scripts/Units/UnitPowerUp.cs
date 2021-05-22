using System.Collections;
using System.Collections.Generic;
using Mirror;
using Pathfinding.RVO;
using UnityEngine;
using UnityEngine.AI;

public class UnitPowerUp : NetworkBehaviour
{
    [SerializeField] private GameObject specialEffectPrefab = null;
    [SerializeField] private BattleFieldRules battleFieldRules = null;
    public bool canSpawnEffect = true;
    bool CanPowerUp = true;
    Unit unit;
    Transform unitTransform;
   
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
    private void Scale()
    {
        transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        GetComponent<IAttack>().ScaleAttackRange(1.5f);
        GetComponent<Unit>().isScaled = true;
        //Debug.Log($"Scale Up {unit.GetComponent<Unit>().unitType}");
    }
    private void Volley()
    {
        GetComponent<UnitFiring>().SetNumberOfShoot(3);
    }
    [ClientRpc]
    private void RpcScale(Transform unitTransform, GameObject unit)
    {
        Scale();
    }
    [Command]
    public void cmdSpeedUp(float speed)
    {
        //Debug.Log($"cmd speed up ? {speed}");
        ServerSetSpeed(speed, true);
    }
    [Server]
    public void ServerSetSpeed(float speed, bool accumulate)
    {
        SetSpeed(speed, accumulate);
        RpcSpeedUp(speed, accumulate);
    }
    public void SetSpeed(float speed, bool accumulate)
    {
        if (speed < 0) { return; }
        if (GetComponent<Unit>().GetUnitMovement().GetSpeed(UnitMeta.SpeedType.CURRENT) < GetComponent<Unit>().GetUnitMovement().GetSpeed(UnitMeta.SpeedType.MAX))
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
    private void SpeedUp(float speed, bool accumulate)
    {
        float currentSpeed = GetComponent<Unit>().GetUnitMovement().GetSpeed(UnitMeta.SpeedType.CURRENT);
        if (accumulate && currentSpeed <= 0.5) { return; }
        GetComponent<Unit>().GetUnitMovement().SetSpeed(UnitMeta.SpeedType.CURRENT, accumulate ? currentSpeed + speed : speed);
    }
    [ClientRpc]
    private void RpcSpeedUp(float speed, bool accumulate)
    {
        SpeedUp(speed, accumulate);
    }
    // ======================================= Special Attack setting ==================================== 
    // Sample Template for Mirror CMD SERVER RPC Usage
    /// <summary>
    /// Do not pass 0 in repeatAttackDelay
    /// </summary>
    public void SpecialEffect(double repeatAttackDelay, int speed)
    {
        if (isServer)
            RpcSpecialEffect(repeatAttackDelay, speed);
        else
            CmdSpecialEffect(repeatAttackDelay, speed);
    }
    /// <summary>
    /// Do not pass 0 in repeatAttackDelay
    /// </summary>
    [Command(ignoreAuthority = true)]
    public void CmdSpecialEffect(double repeatAttackDelay, int speed)
    {
        ServerSpecialEffect(repeatAttackDelay, speed);
    }
    /// <summary>
    /// Do not pass 0 in repeatAttackDelay
    /// </summary>
    [Server]
    public void ServerSpecialEffect(double repeatAttackDelay, int speed)
    {
        HandleSpecialEffect(repeatAttackDelay, speed);
    }
    /// <summary>
    /// Do not pass 0 in repeatAttackDelay
    /// </summary>
    [ClientRpc]
    public void RpcSpecialEffect(double repeatAttackDelay, int speed)
    {
        HandleSpecialEffect(repeatAttackDelay, speed);
    }
    /// <summary>
    /// Do not pass 0 in repeatAttackDelay.
    /// /// if you want to stop the unit attack pass MaxValue
    /// </summary>
    public void HandleSpecialEffect(double repeatAttackDelay, int speed )
    {
        SetSpeed(speed, false);
        //repeatAttackDelay = 0;
        IAttack iSpecialAttack = GetComponent(typeof(IAttack)) as IAttack;
        iSpecialAttack.ChangeAttackDelay(repeatAttackDelay);
    }
    //================================================= End of Special Attack  ===========================================================


    //================================================= Unit Factory Power Up + Card Stats Init  ===========================================================

    public void PowerUp(int playerID, string unitName, int spawnPointIndex, int star, int cardLevel, int health, int attack, float repeatAttackDelay, int speed, int defense, int special, string specialkey, string passivekey, Color teamColor)
    {
        if (isServer) 
            RpcPowerUp(playerID, unitName, spawnPointIndex, star, cardLevel, health, attack, repeatAttackDelay, speed, defense, special, specialkey, passivekey, teamColor);
        else
            CmdPowerUp(playerID, unitName, spawnPointIndex, star, cardLevel, health, attack, repeatAttackDelay, speed, defense, special, specialkey, passivekey, teamColor);
    }
    //[Command(requiresAuthority = false)]
    [Command(ignoreAuthority = true)]
    public void CmdPowerUp(int playerID, string unitName, int spawnPointIndex, int star, int cardLevel, int health, int attack, float repeatAttackDelay, int speed, int defense, int special, string specialkey, string passivekey, Color teamColor)
    {
        //Debug.Log($"CmdPowerUp Speed ==  > {speed}");
        ServerPowerUp(playerID, unitName, spawnPointIndex, star, cardLevel, health, attack, repeatAttackDelay, speed, defense, special, specialkey, passivekey, teamColor);
    }
    [Server]
    public void ServerPowerUp(int playerID, string unitName, int spawnPointIndex, int star, int cardLevel, int health, int attack, float repeatAttackDelay, int speed, int defense, int special, string specialkey, string passivekey, Color teamColor)
    {
        //Debug.Log("ServerpowerUp");
        HandlePowerUp(playerID, unitName, spawnPointIndex, star, cardLevel, health, attack, repeatAttackDelay, speed, defense, special, specialkey, passivekey, teamColor);
        RpcPowerUp(playerID, unitName, spawnPointIndex, star, cardLevel, health, attack, repeatAttackDelay, speed, defense, special, specialkey, passivekey, teamColor);
    }
    public void HandlePowerUp(int playerID, string unitName, int spawnPointIndex, int star,int cardLevel, int health, int attack, float repeatAttackDelay, int speed, int defense, int special, string specialkey, string passivekey, Color teamColor)
    {
        //Debug.Log($"StaticClass.IsFlippedCamera {StaticClass.IsFlippedCamera} , {gameObject.tag} : {gameObject.name} ==> powerUp , star {star} ,cardLevel {cardLevel}, health {health}, attack {attack}, repeatAttackDelay {repeatAttackDelay}, speed {speed}, defense {defense}, special {special} ");
        gameObject.name = unitName;
        gameObject.tag = ((gameObject.GetComponent<Unit>().unitType == UnitMeta.UnitType.KING) ? "King" : "Player") + playerID;
        SetSpeed(speed,false);
        gameObject.GetComponent<CardStats>().SetCardStats(star, cardLevel, health, attack, repeatAttackDelay,  speed,defense, special, specialkey, passivekey);
        gameObject.GetComponent<HealthDisplay>().SetUnitLevel(cardLevel, GetComponent<Unit>().unitType );
        gameObject.GetComponent<Health>().ScaleMaxHealth(health, star);
        gameObject.GetComponent<IAttack>().ScaleDamageDeal(attack, repeatAttackDelay, (star == 1) ? star : (star - 1) * 3);
        //gameObject.GetComponentInChildren<UnitBody>().SetRenderMaterial(playerID , star);
        gameObject.GetComponent<Unit>().SetSpawnPointIndex(spawnPointIndex);
        gameObject.GetComponent<HealthDisplay>().SetHealthBarColor(teamColor);
        GetComponent<RVOController>().layer = tag.Contains("0") ? RVOLayer.Layer3 : RVOLayer.Layer2;
        GetComponent<RVOController>().collidesWith = tag.Contains("0") ? RVOLayer.Layer2 : RVOLayer.Layer3;
        HandleUnitSKill(star);
        if ( StaticClass.IsFlippedCamera ){
            gameObject.GetComponent<HealthDisplay>().flipHealthBar();
        }
    }
    [ClientRpc]
    public void RpcPowerUp(int playerID, string unitName, int spawnPointIndex, int star, int cardLevel, int health, int attack, float repeatAttackDelay, int speed, int defense, int special, string specialkey, string passivekey, Color teamColor)
    {
        //Debug.Log($"{gameObject.tag} : {gameObject.name} RpcPowerUp cardLevel {cardLevel} health {health} speed {speed}");
        HandlePowerUp(playerID, unitName, spawnPointIndex, star, cardLevel, health, attack, repeatAttackDelay, speed, defense, special, specialkey, passivekey, teamColor);
    }
    private void HandleUnitSKill(int star)
    {
        if (gameObject.GetComponent<Unit>().unitType == UnitMeta.UnitType.KING || gameObject.GetComponent<Unit>().unitType == UnitMeta.UnitType.HERO) { return; } 
        UnitMeta.UnitSkill skill = UnitMeta.UnitStarSkill[star][gameObject.GetComponent<Unit>().unitType];
        //Debug.Log($" star {star} unitType {gameObject.GetComponent<Unit>().unitType} skill {skill} ");
        switch (skill)
        {
            case UnitMeta.UnitSkill.SCALE:
                Scale();
                break;
            case UnitMeta.UnitSkill.VOLLEY:
                Volley();
                break;
            case UnitMeta.UnitSkill.NOTHING:
            default:
                break;
        }
    }

    //======================================================== End of Unit Factory   ================================================================

}
