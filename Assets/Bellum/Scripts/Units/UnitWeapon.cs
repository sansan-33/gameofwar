using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tactical;
using Mirror;
using UnityEngine;

public class UnitWeapon : NetworkBehaviour, IAttackAgent, IAttack
{
    
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private float damageToDeal = 1;
    
    [SerializeField] private GameObject attackPoint;
    [SerializeField] private float attackRange=5f;
    [SerializeField] public LayerMask layerMask = new LayerMask();
    [SerializeField] private GameObject specialEffectPrefab  = null;
    [SerializeField] private GameObject slashEffectPrefab = null;
    [SerializeField] private bool IsAreaOfEffect = false;
    private Vector3 weaponSize = new Vector3(0.15f, 0.1f, 0.5f);
    private float calculatedDamageToDeal ;
    private float originalDamage;
    [SerializeField] private float DashDamageFactor = 0.1f;
    public bool IsKingSP = false;
    NetworkIdentity opponentIdentity;
    bool m_Started;
    // The amount of time it takes for the agent to be able to attack again
    public float repeatAttackDelay;
    // The maximum angle that the agent can attack from
    public float attackAngle;

    // The last time the agent attacked
    private float lastAttackTime;
    RTSPlayer player;
    float upGradeAmount =  1.01f;
    [SerializeField] private GameObject textPrefab = null;
    public bool AUTOFIRE = false;

    private Unit unit;
    public override void OnStartAuthority()
    {
        if (NetworkClient.connection.identity == null) { return; }
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        calculatedDamageToDeal = damageToDeal;
        //Use this to ensure that the Gizmos are being drawn when in Play Mode.
        m_Started = true;
        originalDamage = damageToDeal;
        if (AUTOFIRE) StartCoroutine(autoFire());
    }
    IEnumerator autoFire()
    {
        while (true)
        {
            yield return new WaitForSeconds(repeatAttackDelay);
            yield return TryAttack();
        }
        //yield return null;
    }
    // Commands are sent from player objects on the client to player objects on the server
    // IF SERVER SIDE , object refernce not found exception
    //
    public IEnumerator TryAttack()
    {
        //if (player.GetPlayerID() == 1) Debug.Log($"Attacker {targeter} attacking .... ");
        unit = GetComponent<Unit>();
        calculatedDamageToDeal = damageToDeal;
        //Use the OverlapBox to detect if there are any other colliders within this box area.
        //Use the GameObject's centre, half the size (as a radius) and rotation. This creates an invisible box around your GameObject.
        Collider[] hitColliders = Physics.OverlapBox(attackPoint.transform.position, weaponSize * attackRange, transform.rotation, layerMask);
        int i = 0;
        Collider other;
        bool isFlipped = false;
        GameObject firstOther = null;
        //Check when there is a new collider coming into contact with the box
        while (i < hitColliders.Length)
        {
            other = hitColliders[i++];
            if (other != null && (other.tag.Contains("Building") || other.tag == "Wall") ) { break; } // Hit wall just break the loop and wait for next attack
            if (other == null || !other.GetComponent<Health>().IsAlive()) { continue; }
            //((RTSNetworkManager)NetworkManager.singleton).Players
            isFlipped = false;
            if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)
            {
                if ( other.tag.Substring(other.tag.Length - 1 ) == targeter.tag.Substring(targeter.tag.Length - 1)) { continue; }  //check to see if it belongs to the player, if it does, do nothing
            }
            else // Multi player seneriao
            {
                //if (player.GetPlayerID() == 0)
                //{
                    isFlipped = true;
               // }
                //Debug.Log($"Multi player seneriao ");
                if (other.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity))  //try and get the NetworkIdentity component to see if it's a unit/building 
                {
                    if (networkIdentity.hasAuthority) { continue; }  //check to see if it belongs to the player, if it does, do nothing
                }
            }
         
            if (other.TryGetComponent<Health>(out Health health))
            {
                if(player == null) {player = NetworkClient.connection.identity.GetComponent<RTSPlayer>(); }
                //Debug.Log($"{player}{GetComponent<NetworkIdentity>()}{other}");
                //Debug.Log($"{player.GetPlayerID()}{GetComponent<NetworkIdentity>()}{other.GetComponent<NetworkIdentity>()}");
                opponentIdentity = (player.GetPlayerID() == 1) ? GetComponent<NetworkIdentity>() : other.GetComponent<NetworkIdentity>();
                //Debug.Log($"Original damage {damageToDeal}, {this.GetComponent<Unit>().unitType} , {other.GetComponent<Unit>().unitType} ");
                calculatedDamageToDeal = StrengthWeakness.calculateDamage(unit.unitType, other.GetComponent<Unit>().unitType, damageToDeal);
                if (unit.unitType == UnitMeta.UnitType.CAVALRY && unit.GetUnitMovement().GetAcceleration() > 0f)
                {
                    Debug.Log($"Unit {name} Weapon speed current: {unit.GetUnitMovement().GetSpeed(UnitMeta.SpeedType.CURRENT)}  max: {unit.GetUnitMovement().GetSpeed(UnitMeta.SpeedType.MAX)} calculatedDamageToDeal {calculatedDamageToDeal} * {DashDamageFactor}");
                    calculatedDamageToDeal *= (DashDamageFactor * unit.GetUnitMovement().GetSpeed(UnitMeta.SpeedType.CURRENT));
                    cmdDamageText(other.transform.position, calculatedDamageToDeal, originalDamage, opponentIdentity, isFlipped);
                }
                //yield return new WaitForSeconds(GetComponent<IAttack>().RepeatAttackDelay() - .6f);
                yield return null;
                if (other == null || !health.IsAlive()) { continue; }
                CmdDealDamage(other.gameObject, calculatedDamageToDeal, targeter.tag.Substring(targeter.tag.Length - 1));
                if (IsKingSP == true)
                {
                    cmShake();
                    ReScaleDamageDeal();
                }
                cmdSpecialEffect(other.transform.position);
                if(firstOther == null)
                    firstOther = other.gameObject;
                if ( UnitMeta.ShakeCamera.ContainsKey (UnitMeta.GetUnitKeyByRaceType(unit.race, unit.unitType))) { cmShake(); }
                if (tag.Contains("Sneaky")) GetComponent<UnitPowerUp>().CmdSneakOff();
                if (!IsAreaOfEffect)
                    break;
            }

        }
        if (IsAreaOfEffect && firstOther !=null)
            cmdSlashEffect(firstOther.GetComponent<Targeter>().GetAimAtPoint().position);


    }
    //Draw the Box Overlap as a gizmo to show where it currently is testing. Click the Gizmos button to see this
    void OnDrawGizmos()
    {
        Color prevColor = Gizmos.color;
        Matrix4x4 prevMatrix = Gizmos.matrix;

        //Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode
        if (m_Started)
        {
            // cache previous Gizmos settings
            
            if (name.Contains("Tapir")) { Gizmos.DrawWireCube(attackPoint.transform.position, weaponSize * attackRange); Gizmos.color = Color.green; } else
            {
                Gizmos.color = Color.yellow;
                Gizmos.matrix = transform.localToWorldMatrix;
                Vector3 boxPosition = attackPoint.transform.position;
                // convert from world position to local position 
                boxPosition = transform.InverseTransformPoint(boxPosition);
                Gizmos.DrawWireCube(boxPosition, weaponSize * attackRange);
                // restore previous Gizmos settings
                Gizmos.color = prevColor;
                Gizmos.matrix = prevMatrix;
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdDealDamage(GameObject enemy,  float damage, string _playerid)
    {

        
        string color = _playerid == "0" ? "blue" : "red";
        //Debug.Log($"Cmd Deal Damage color : {color} ");
        if (enemy.GetComponent<Health>().DealDamage(damage)){
            KilledEnemy();
            if(enemy.GetComponent<Unit>().unitType == UnitMeta.UnitType.DOOR)
            {
                GreatWallController wallController = GameObject.FindGameObjectWithTag("GreatWallController").GetComponent<GreatWallController>();
                wallController.dynamicBlock(true);
                if (enemy.GetComponent<UnitBody>() != null)
                {
                    //Debug.Log("Gate Open in unit weapon");
                    enemy.GetComponent<UnitBody>().SetTeamColor(color);
                    //GateOpened?.Invoke( "" + _playerid);
                    wallController.GateOpen("" + _playerid, enemy.GetComponent<UnitBody>().doorIndex.ToString() );
                }
            }
            if(enemy.GetComponent<Unit>().unitType == UnitMeta.UnitType.STUPIDTAPIR)
            {
                ChangeTag(enemy);
            }
        }
    }
    private void ChangeTag(GameObject enemy)
    {
        int ID = tag.Contains("0") ? 0 : 1;
        //Debug.Log($"change tag to {ID }");
        enemy.tag = "Player" + ID;
        enemy.name = "StupidTapir";
    }
    private void KilledEnemy()
    {
        if (isServer)
            RpcpowerUpAfterKill(gameObject);
        else
            cmdPowerUpAfterKill(gameObject);
    }
    [Command(requiresAuthority = false)]
    private void cmdPowerUpAfterKill(GameObject unit)
    {
        ServerPowerUpAfterKill(unit);
    }
    [Server]
    private void ServerPowerUpAfterKill(GameObject unit)
    {
        powerUpAfterKill(unit);
    }
    [Command]
    private void cmdDamageText(Vector3 targetPos, float damageNew, float damgeOld, NetworkIdentity opponentIdentity, bool flipText)
    {
        targetPos.x = targetPos.x + 10;
        targetPos.y = targetPos.y + 5;
        GameObject floatingText = SetupDamageText(targetPos, damageNew, damgeOld);
        NetworkServer.Spawn(floatingText, connectionToClient);
        
        if (opponentIdentity == null) { return; }
     
        if (flipText) { TargetCommandText(opponentIdentity.connectionToClient, floatingText, opponentIdentity); }
    }
    [Command]
    private void cmdSpecialEffect(Vector3 position  )
    {
        Debug.Log($"{name } instabtiate {specialEffectPrefab}");
        GameObject effect = Instantiate(specialEffectPrefab,  position, Quaternion.Euler(new Vector3(0, 0, 0)));
        NetworkServer.Spawn(effect, connectionToClient);
    }
    [Command]
    private void cmdSlashEffect(Vector3 position )
    {
        if (slashEffectPrefab == null) { return; }
        Quaternion rotation = Quaternion.LookRotation(position - transform.position);
        //Debug.Log($"{name} rotation {rotation} y {rotation.y},  position : {position - transform.position}");
        GameObject effect = Instantiate(slashEffectPrefab, position, Quaternion.Euler(new Vector3(0, rotation.y > 0 ? 0 : 180, 0)));
        NetworkServer.Spawn(effect, connectionToClient);
    }
    public void cmShake()
    {
        CinemachineManager cmManager = GameObject.FindGameObjectWithTag("CinemachineManager").GetComponent<CinemachineManager>();
        cmManager.shake();
    }
    private void cmFreeLook()
    {
        CinemachineManager cmManager = GameObject.FindGameObjectWithTag("CinemachineManager").GetComponent<CinemachineManager>();
        cmManager.ThirdCamera(GameObject.FindGameObjectWithTag("Player" + player.GetPlayerID()), GameObject.FindGameObjectWithTag("Player" + player.GetEnemyID()));
    }
    public Transform AttackPoint()
    {
        return attackPoint.transform;
    }
    public float AttackDistance()
    {
        return attackRange;
    }
    public float RepeatAttackDelay()
    {
        return repeatAttackDelay;
    }
    public bool CanAttack()
    {
        //Debug.Log($"{lastAttackTime} + {repeatAttackDelay} < {Time.time} ");
        return lastAttackTime + repeatAttackDelay < Time.time;
    }
    
    public float AttackAngle()
    {
        return attackAngle;
    }

    public void Attack(Vector3 targetPosition)
    {
        lastAttackTime = Time.time;
        //Debug.Log($"Unit Weapon ==> unit {targeter.transform.GetComponent<Unit>().name } attacking now, lastAttackTime: {lastAttackTime} ");
        
        targeter.transform.GetComponent<UnitAnimator>().StateControl(UnitAnimator.AnimState.ATTACK);
        StartCoroutine(TryAttack());
    }
    public void ScaleAttackDelay(float factor)
    {
        repeatAttackDelay =  repeatAttackDelay * factor ;
    }
    /// <summary>
    /// Do not pass 0
    /// </summary>
    /// <param name="channgeValue"></param>
    public void ChangeAttackDelay(double channgeValue)
    {
        //channgeValue = 1;
        repeatAttackDelay = (float)channgeValue;
    }

    public void ScaleDamageDeal(int attack, float repeatAttackDelay, float factor)
    {
        damageToDeal = attack == 0 ? damageToDeal : attack;
        this.repeatAttackDelay = repeatAttackDelay == 0 ? this.repeatAttackDelay : repeatAttackDelay;
        damageToDeal =  (int)  (damageToDeal * factor);
    }
    public void ReScaleDamageDeal()
    {
        damageToDeal = originalDamage;
    }
    public void ScaleAttackRange(float factor)
    {
        attackRange = attackRange * factor;
    }
    public void powerUpAfterKill(GameObject unit)
    {
        unit.GetComponent<HealthDisplay>().HandleKillText(1);
        damageToDeal *= upGradeAmount;
    }
    [ClientRpc]
    public void RpcpowerUpAfterKill(GameObject unit)
    {
        //Debug.Log($"{name} client rpc power up {unit.name}");
        unit.GetComponent<HealthDisplay>().HandleKillText(1);
        damageToDeal *= upGradeAmount;
    }
    [TargetRpc]
    public void TargetCommandText(NetworkConnection other , GameObject floatingText, NetworkIdentity others)
    {
        //Debug.Log("TargetCommandText");
        floatingText.GetComponent<DamageTextHolder>().displayRotation.y = 180; 
    }
    private GameObject SetupDamageText(Vector3 targetPos, float damageToDeals, float damageToDealOriginal)
    {
        GameObject floatingText = Instantiate(textPrefab, targetPos, Quaternion.identity);
        floatingText.transform.position = targetPos;
        floatingText.transform.rotation = Quaternion.identity;
        Color textColor;
        string dmgText;
        if (damageToDeals > damageToDealOriginal)
        {
            textColor = floatingText.GetComponent<DamageTextHolder>().CriticalColor;
            dmgText = damageToDeals + " Critical";
        }
        else
        {
            textColor = floatingText.GetComponent<DamageTextHolder>().NormalColor;
            dmgText = damageToDeals + "";
        }
        floatingText.GetComponent<DamageTextHolder>().displayColor = textColor;
        floatingText.GetComponent<DamageTextHolder>().displayText = dmgText;
        return floatingText;
    }
    
  
}
