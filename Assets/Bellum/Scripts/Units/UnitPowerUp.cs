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
    [SerializeField] private Material sneakyMaterial = null;
    public bool canSpawnEffect = true;
    private Material[] origialMaterial;

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

    [Command]
    public void CmdAccelerate(float acceleration)
    {
        ServerAccelerate(acceleration);
    }
    [Server]
    public void ServerAccelerate(float acceleration)
    {
        Accelerate(acceleration);
    }
    public void SetAcceleration(float acceleration)
    {
        if (acceleration < 0) { return; }
        if (isServer)
            RpcAccelerate(acceleration);
        else
            CmdAccelerate(acceleration);
    }
    private void Accelerate(float acceleration)
    {
        GetComponent<Unit>().GetUnitMovement().SetAcceleration(acceleration);
    }
    [ClientRpc]
    private void RpcAccelerate(float acceleration )
    {
        Accelerate(acceleration);
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
        //RpcPowerUp(playerID, unitName, spawnPointIndex, star, cardLevel, health, attack, repeatAttackDelay, speed, defense, special, specialkey, passivekey, teamColor);
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
        GetComponent<RVOController>().collidesWith = tag.Contains("0") ? RVOLayer.Layer3 : RVOLayer.Layer2;
        //GetComponent<RVOController>().collidesWith = tag.Contains("0") ? RVOLayer.Layer2 : RVOLayer.Layer3;
        HandleUnitSkill(UnitMeta.UnitSkill.DEFAULT, star, attack, repeatAttackDelay,speed);
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
    private void HandleUnitSkill(UnitMeta.UnitSkill skill, int star, int attack, float repeatAttackDelay, float speed)
    {
        UnitMeta.UnitType unitType = gameObject.GetComponent<Unit>().unitType;
        if (unitType == UnitMeta.UnitType.KING || unitType == UnitMeta.UnitType.HERO
            || unitType == UnitMeta.UnitType.TOWER || unitType == UnitMeta.UnitType.BARRACK
            || unitType == UnitMeta.UnitType.CATAPULT || unitType == UnitMeta.UnitType.WALL ) { return; }
        //Debug.Log($" star {star} unitType {gameObject.GetComponent<Unit>().unitType} skill {skill} ");
        UnitMeta.UnitSkill defaultSkill = skill == UnitMeta.UnitSkill.DEFAULT ? UnitMeta.UnitStarSkill[star][gameObject.GetComponent<Unit>().unitType] : skill;
        //Debug.Log($" star {star} unitType {gameObject.GetComponent<Unit>().unitType} skill {skill} ");
        switch (defaultSkill)
        {
            case UnitMeta.UnitSkill.SCALE:
                Scale();
                break;
            case UnitMeta.UnitSkill.SHIELD:
                Provoke();
                Shield();
                break;
            case UnitMeta.UnitSkill.VOLLEY:
                Volley();
                break;
            case UnitMeta.UnitSkill.ARROWRAIN:
                ArrowRain();
                break;
            case UnitMeta.UnitSkill.PROVOKE:
                Provoke();
                break;
            case UnitMeta.UnitSkill.TORNADO:
                Healing();
                Tornado();
                break;
            case UnitMeta.UnitSkill.HEAL:
                Healing();
                break;
            case UnitMeta.UnitSkill.CHARGE:
                Dashing(0.5f);
                Charging(attack, repeatAttackDelay);
                break;
            case UnitMeta.UnitSkill.DASH:
                Dashing(0.5f);
                break;
            case UnitMeta.UnitSkill.SNEAK:
                Sneak(attack, repeatAttackDelay);
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
    private void ArrowRain()
    {
        if (gameObject == null || GetComponent<UnitFiring>() == null || GetComponent<IAttack>() == null) { return; }
        GetComponent<UnitFiring>().SetNumberOfShoot(2);
        GetComponent<IAttack>().ScaleAttackRange(20f);
        //Debug.Log($"{name} ArrowRain");       
    }
    private void Shield()
    {
        ShieldAura shield = GetComponentInChildren<ShieldAura>();
        if (shield == null) { return; }
        shield.aura();
        
    }
    private void Provoke()
    {
        //GetComponent<UnitFiring>().SetNumberOfShoot(3);
        GetComponent<UnitAnimator>().StateControl(UnitAnimator.AnimState.PROVOKE);
        gameObject.tag = "Provoke" + tag.Substring(tag.Length - 1);
        fxEffect(GetComponentInParent<Transform>());
    }
    private void Healing()
    {
        if(TryGetComponent(out Healing healing))
        GetComponent<Healing>().ServerEnableHealing(true);
    }
    private void Dashing(float acceleration)
    {
        //SetSpeed(speed * 1.5f, false);
        SetAcceleration(acceleration);
        GameObject specialEffect = Instantiate(specialEffectPrefab, GetComponentInParent<Transform>());
        NetworkServer.Spawn(specialEffect, connectionToClient);
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
    private void Sneak(int attack, float repeatAttackDelay)
    {
        gameObject.tag = "Sneaky" + tag.Substring(tag.Length - 1);
        gameObject.GetComponent<IAttack>().ScaleDamageDeal(attack, repeatAttackDelay, 99);
        foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            //Material[]  mats = new Material[] {sneakyMaterial, skinnedMeshRenderer.materials[1] };
            origialMaterial = skinnedMeshRenderer.materials;
            Material[]  mats = new Material[] {sneakyMaterial};
            skinnedMeshRenderer.materials = mats;
        }
    }
    [Command]
    public void CmdSneakOff()
    {
        gameObject.tag = "Player" + tag.Substring(tag.Length - 1);
        foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            skinnedMeshRenderer.materials = origialMaterial;
        }
    }
    private void Charging(int attack, float repeatAttackDelay)
    {
        gameObject.GetComponent<IAttack>().ScaleDamageDeal(attack, repeatAttackDelay, 3);
        Debug.Log($"Charging attack {attack} repeatAttackDelay {repeatAttackDelay}");
        fxEffect(GetComponent<IAttack>().AttackPoint());
        //GameObject fxEffect = Instantiate(fxEffectPrefab, GetComponent<IAttack>().AttackPoint());
        //NetworkServer.Spawn(fxEffect, connectionToClient);
    }
    private void fxEffect(Transform transform)
    {
        GameObject fxEffect = Instantiate(fxEffectPrefab, transform);
        NetworkServer.Spawn(fxEffect, connectionToClient);
    }
    //======================================================== End of Unit Factory   ================================================================

    //==================================== Set Tag For DIE
    public void SetTag(string tag)    {
        if (isServer)
            RpcSetTag(tag);
        else
            CmdSetTag(tag);
    }
    [Command]
    public void CmdSetTag(string tag)
    {
        ServerSetTag(tag);
    }
    [ClientRpc]
    public void RpcSetTag(string tag)
    {
        HandleSetTag(tag);
    }
    [Server]
    public void ServerSetTag(string tag)
    {
        HandleSetTag(tag);
    }
    private void HandleSetTag(string tag)
    {
        gameObject.tag = tag;
    }
    //==================================== Set Skill For Unit
    public void SetSkill(UnitMeta.UnitSkill skill, int star, int attack, float repeatAttackDelay, float speed)
    {
        if (isServer)
            RpcSetSkill(skill, star, attack, repeatAttackDelay, speed);
        else
            CmdSetSkill(skill, star, attack, repeatAttackDelay, speed);
    }
    [Command]
    public void CmdSetSkill(UnitMeta.UnitSkill skill,int star, int attack, float repeatAttackDelay, float speed)
    {
        ServerSetSkill(skill, star, attack, repeatAttackDelay, speed);
    }
    [ClientRpc]
    public void RpcSetSkill(UnitMeta.UnitSkill skill,int star, int attack, float repeatAttackDelay, float speed)
    {
        HandleSetSkill(skill, star, attack, repeatAttackDelay, speed);
    }
    [Server]
    public void ServerSetSkill(UnitMeta.UnitSkill skill, int star, int attack, float repeatAttackDelay, float speed)
    {
        HandleSetSkill(skill, star, attack, repeatAttackDelay, speed);
    }
    private void HandleSetSkill(UnitMeta.UnitSkill skill, int star, int attack, float repeatAttackDelay, float speed)
    {
        HandleUnitSkill(skill, star, attack, repeatAttackDelay, speed);
    }
}
