using System;
using System.Collections;
using System.Collections.Generic;
using FoW;
using Mirror;
using Pathfinding;
using Pathfinding.RVO;
using UnityEngine;
using UnityEngine.AI;

public class UnitPowerUp : NetworkBehaviour
{
    [SerializeField] private GameObject specialEffectPrefab = null;
    [SerializeField] private GameObject fxEffectPrefab = null;
    [SerializeField] private GameObject scriptEffectPrefab = null;
    GameObject fxEffectObj;
    GameObject specialEffectObj;
    [SyncVar]
    private bool isFXPlay = false;
    [SyncVar]
    private bool isSpecialEffectPlay = false;
    [SerializeField] private Material sneakyMaterial = null;
    public bool canSpawnEffect = true;
    private Material[] origialMaterial;
    private HashSet<UnitMeta.UnitSkill> activeSkill = new HashSet<UnitMeta.UnitSkill>();

    // Start is called before the first frame update
    void Start()
    {
        spawnFxEffect();
        spawnSpecialEffect();
    }
    private void Update()
    {
        bool fxLock = true;
        bool specialLock = true;

        if (isFXPlay && fxLock)
        {
            fxPlay();
            fxLock = false;
        }
        if (isSpecialEffectPlay && specialLock)
        {
            specialEffectPlay();
            specialLock = false;
        }
    }
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
    public void SpecialEffect(double repeatAttackDelay, float speed)
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

    public void CmdSpecialEffect(double repeatAttackDelay, float speed)
    {
        ServerSpecialEffect(repeatAttackDelay, speed);
    }
    /// <summary>
    /// Do not pass 0 in repeatAttackDelay
    /// </summary>
    [Server]
    public void ServerSpecialEffect(double repeatAttackDelay, float speed)
    {
        HandleSpecialEffect(repeatAttackDelay, speed);
    }
    /// <summary>
    /// Do not pass 0 in repeatAttackDelay
    /// </summary>
    [ClientRpc]
    public void RpcSpecialEffect(double repeatAttackDelay, float speed)
    {
        HandleSpecialEffect(repeatAttackDelay, speed);
    }
    /// <summary>
    /// Do not pass 0 in repeatAttackDelay.
    /// /// if you want to stop the unit attack pass MaxValue
    /// </summary>
    public void HandleSpecialEffect(double repeatAttackDelay, float speed )
    {
        SetSpeed(speed, false);
        //repeatAttackDelay = 0;
        IAttack iSpecialAttack = GetComponent(typeof(IAttack)) as IAttack;
        iSpecialAttack.ChangeAttackDelay(repeatAttackDelay);
    }
    //================================================= End of Special Attack  ===========================================================


    //================================================= Unit Factory Power Up + Card Stats Init  ===========================================================

    public void PowerUp(int playerID, string unitName, int spawnPointIndex, int star, int cardLevel, int health, int attack, float repeatAttackDelay, float speed, int defense, int special, string specialkey, string passivekey, Color teamColor)
    {
        if (isServer) 
            RpcPowerUp(playerID, unitName, spawnPointIndex, star, cardLevel, health, attack, repeatAttackDelay, speed, defense, special, specialkey, passivekey, teamColor);
        else
            CmdPowerUp(playerID, unitName, spawnPointIndex, star, cardLevel, health, attack, repeatAttackDelay, speed, defense, special, specialkey, passivekey, teamColor);
    }
    //[Command(ignoreAuthority = true)]
    [Command(requiresAuthority = false)]
    public void CmdPowerUp(int playerID, string unitName, int spawnPointIndex, int star, int cardLevel, int health, int attack, float repeatAttackDelay, float speed, int defense, int special, string specialkey, string passivekey, Color teamColor)
    {
        //Debug.Log($"CmdPowerUp Speed ==  > {speed}");
        ServerPowerUp(playerID, unitName, spawnPointIndex, star, cardLevel, health, attack, repeatAttackDelay, speed, defense, special, specialkey, passivekey, teamColor);
    }
    [Server]
    public void ServerPowerUp(int playerID, string unitName, int spawnPointIndex, int star, int cardLevel, int health, int attack, float repeatAttackDelay, float speed, int defense, int special, string specialkey, string passivekey, Color teamColor)
    {
        //Debug.Log("ServerpowerUp");
        HandlePowerUp(playerID, unitName, spawnPointIndex, star, cardLevel, health, attack, repeatAttackDelay, speed, defense, special, specialkey, passivekey, teamColor);
        //if comment this line . player 2 name is lower case with (clone) , card stats and other things not sync
        RpcPowerUp(playerID, unitName, spawnPointIndex, star, cardLevel, health, attack, repeatAttackDelay, speed, defense, special, specialkey, passivekey, teamColor);
    }
    public void HandlePowerUp(int playerID, string unitName, int spawnPointIndex, int star,int cardLevel, int health, int attack, float repeatAttackDelay, float speed, int defense, int special, string specialkey, string passivekey, Color teamColor)
    {
        //Debug.Log($"HandlePowerUp StaticClass.IsFlippedCamera {StaticClass.IsFlippedCamera} , {gameObject.tag} : {gameObject.name}/{unitName} ==> powerUp , star {star} ,cardLevel {cardLevel}, health {health}, attack {attack}, repeatAttackDelay {repeatAttackDelay}, speed {speed}, defense {defense}, special {special} ");
        gameObject.name = unitName;
        //Debug.Log($"HandlePowerUp this.name {this.name}, unitName {unitName}, gameObject {gameObject.name}");
        gameObject.tag = ((gameObject.GetComponent<Unit>().unitType == UnitMeta.UnitType.KING) ? "King" : "Player") + playerID;

        var mask = gameObject.GetComponent<Seeker>().traversableTags;
        var enemyid = playerID == 0 ? 1 : 0;
        gameObject.GetComponent<Seeker>().traversableTags = mask & ~(1 << (enemyid + 1));
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
        GetComponent<FogOfWarUnit>().team = playerID;
        GetComponent<FogOfWarUnit>().circleRadius  = GetComponent<IAttack>().AttackDistance();
        HandleUnitSkill(UnitMeta.UnitSkill.DEFAULT, star, attack, repeatAttackDelay,speed);
        if ( StaticClass.IsFlippedCamera ){
            gameObject.GetComponent<HealthDisplay>().flipHealthBar();
        }
        //Debug.Log($"HandlePowerUp this.name {this.name}, unitName {unitName}, gameObject {gameObject.name}");

    }
    [ClientRpc]
    public void RpcPowerUp(int playerID, string unitName, int spawnPointIndex, int star, int cardLevel, int health, int attack, float repeatAttackDelay, float speed, int defense, int special, string specialkey, string passivekey, Color teamColor)
    {
        //Debug.Log($"RpcPowerUp {gameObject.tag} : {gameObject.name} RpcPowerUp cardLevel {cardLevel} health {health} speed {speed}");
        HandlePowerUp(playerID, unitName, spawnPointIndex, star, cardLevel, health, attack, repeatAttackDelay, speed, defense, special, specialkey, passivekey, teamColor);
    }
    private void HandleUnitSkill(UnitMeta.UnitSkill skill, int star, int attack, float repeatAttackDelay, float speed)
    {
        UnitMeta.UnitType unitType = gameObject.GetComponent<Unit>().unitType;
        if (unitType == UnitMeta.UnitType.KING || unitType == UnitMeta.UnitType.HERO || unitType == UnitMeta.UnitType.QUEEN
            || unitType == UnitMeta.UnitType.TOWER || unitType == UnitMeta.UnitType.BARRACK
            || unitType == UnitMeta.UnitType.CATAPULT || unitType == UnitMeta.UnitType.WALL ) { return; }
        //Debug.Log($" star {star} unitType {gameObject.GetComponent<Unit>().unitType} skill {skill} ");
        UnitMeta.UnitSkill defaultSkill = skill == UnitMeta.UnitSkill.DEFAULT ? UnitMeta.UnitStarSkill[star][gameObject.GetComponent<Unit>().unitType] : skill;
        //Debug.Log($" star {star} unitType {gameObject.GetComponent<Unit>().unitType} skill {skill} ");

        if (activeSkill.Contains(defaultSkill)) { return; }
        activeSkill.Add(defaultSkill);
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
        isFXPlay = true;
        //fxEffect(GetComponentInParent<Transform>());
    }
    private void Healing()
    {
        if(TryGetComponent(out Healing healing))
        GetComponent<Healing>().enableHealing(true);
    }
    private void Dashing(float acceleration)
    {
        //SetSpeed(speed * 1.5f, false);
        SetAcceleration(acceleration);
        isFXPlay = true;
        //isSpecialEffectPlay = true;
     }
    private void Tornado()
    {
        float offset = tag.Contains("0") ? 10f : -5f;
        Transform transform = GetComponentInParent<Transform>();
        Vector3 position = new Vector3 (transform.position.x, transform.position.y, transform.position.z + offset);
        GameObject scriptEffect = Instantiate(scriptEffectPrefab, position, Quaternion.identity);
        scriptEffect.GetComponent<Tornado>().SetPlayerType(Int32.Parse(tag.Substring(tag.Length - 1)));
        NetworkServer.Spawn(scriptEffect, connectionToClient);
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
        isSpecialEffectPlay = true;
        isFXPlay = true;
        //fxEffect(GetComponent<IAttack>().AttackPoint());
    }
    private void spawnFxEffect()
    {
        if (fxEffectPrefab == null) { return; }
        Transform transform;
        if (GetComponent<Unit>().unitType == UnitMeta.UnitType.CAVALRY)
            transform = GetComponent<IAttack>().AttackPoint();
        else
            transform = GetComponentInParent<Transform>();
        fxEffectObj = Instantiate(fxEffectPrefab, transform);
        //NetworkServer.Spawn(fxEffectObj, connectionToClient);
    }
    private void spawnSpecialEffect()
    {
        if (specialEffectPrefab == null) { return; }
        specialEffectObj = Instantiate(specialEffectPrefab, transform);
    }
    private void fxPlay()
    {
        fxEffectObj.GetComponent<ParticleSystem>().Play();
    }
    private void specialEffectPlay()
    {
        //Debug.Log($"{name} specialEffectPlay");
        specialEffectObj.GetComponent<ParticleSystem>().Play();
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
