using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Pathfinding.RVO;
using UnityEngine;
using UnityEngine.AI;

public class UnitPowerUp : NetworkBehaviour
{
    [SerializeField] private GameObject specialEffectPrefab = null;
    [SerializeField] private GameObject fxEffectPrefab = null;
    public bool canSpawnEffect = true;
  
    private void Scale()
    {
        transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        GetComponent<IAttack>().ScaleAttackRange(1.5f);
        GetComponent<Unit>().isScaled = true;
        //Debug.Log($"Scale Up {unit.GetComponent<Unit>().unitType}");
    }
    [ClientRpc]
    private void RpcScale(Transform unitTransform, GameObject unit)
    {
        Scale();
    }
    [Command]
    public void CmdSpeedUp(float speed, bool accumulate)
    {
        ServerSetSpeed(speed, accumulate);
    }
    [Server]
    public void ServerSetSpeed(float speed, bool accumulate)
    {
        SpeedUp(speed, accumulate);
        //RpcSpeedUp(speed, accumulate);
    }
    public void SetSpeed(float speed, bool accumulate)
    {
        if (speed < 0) { return; }
        if (GetComponent<Unit>().GetUnitMovement().GetSpeed(UnitMeta.SpeedType.CURRENT) < GetComponent<Unit>().GetUnitMovement().GetSpeed(UnitMeta.SpeedType.MAX))
        {
            if (isServer)
                RpcSpeedUp(speed, accumulate);
            else
                CmdSpeedUp(speed, accumulate);
            //SpeedUp(speed, accumulate);
        }
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
    //[Command(ignoreAuthority = true)]
    [Command(requiresAuthority = false)]

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
    //[Command(ignoreAuthority = true)]
    [Command(requiresAuthority = false)]
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
        HandleUnitSKill(star, attack, repeatAttackDelay);
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
    private void HandleUnitSKill(int star, int attack, float repeatAttackDelay)
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
            case UnitMeta.UnitSkill.PROVOKE:
                Provoke();
                break;
            case UnitMeta.UnitSkill.TORNADO:
                Tornado();
                break;
            case UnitMeta.UnitSkill.HEAL:
                Healing();
                break;
            case UnitMeta.UnitSkill.CHARGE:
                Charging(attack, repeatAttackDelay);
                break;
            case UnitMeta.UnitSkill.NOTHING:
            default:
                break;
        }
    }
    private void Volley()
    {
        GetComponent<UnitFiring>().SetNumberOfShoot(3);
    }
    private void Provoke()
    {
        //GetComponent<UnitFiring>().SetNumberOfShoot(3);
        gameObject.tag = "Provoke" + tag.Substring(tag.Length - 1);
        fxEffect();
    }
    private void Healing()
    {
        if(TryGetComponent(out Healing healing))
        GetComponent<Healing>().ServerEnableHealing(true);
    }
    private void Tornado()
    {
        float offset = tag.Contains("0") ? 10f : -10f;
        Transform transform = GetComponentInParent<Transform>();
        Vector3 position = new Vector3 (transform.position.x, transform.position.y, transform.position.z + offset);
        GameObject specialEffect = Instantiate(specialEffectPrefab, position, Quaternion.identity);
        specialEffect.GetComponent<Tornado>().SetPlayerType(Int32.Parse(tag.Substring(tag.Length - 1)));
        NetworkServer.Spawn(specialEffect, connectionToClient);
    }
    private void Charging(int attack, float repeatAttackDelay)
    {
        gameObject.GetComponent<IAttack>().ScaleDamageDeal(attack, repeatAttackDelay, 3);
        Debug.Log($"Charging attack {attack} repeatAttackDelay {repeatAttackDelay}");
        GameObject fxEffect = Instantiate(fxEffectPrefab, GetComponent<IAttack>().AttackPoint());
        NetworkServer.Spawn(fxEffect, connectionToClient);
    }
    private void fxEffect()
    {
        GameObject fxEffect = Instantiate(fxEffectPrefab, GetComponentInParent<Transform>());
        NetworkServer.Spawn(fxEffect, connectionToClient);
    }
    //======================================================== End of Unit Factory   ================================================================

}
