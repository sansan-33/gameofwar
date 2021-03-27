using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tactical;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class UnitWeapon : NetworkBehaviour, IAttackAgent, IAttack
{
    
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private float damageToDeal = 1;
    
    [SerializeField] private GameObject camPrefab = null;
    [SerializeField] private GameObject camFreeLookPrefab = null;
    [SerializeField] private GameObject attackPoint;
    [SerializeField] private float attackRange=5f;
    [SerializeField] public LayerMask layerMask = new LayerMask();
    [SerializeField] private GameObject specialEffectPrefab  = null;
    [SerializeField] private bool IsAreaOfEffect = false;
    private float calculatedDamageToDeal ;
    private float originalDamage;
    public float DashDamage = 0;
    public bool IsKingSP = false;
    NetworkIdentity opponentIdentity;
    bool m_Started;
    // The amount of time it takes for the agent to be able to attack again
    public float repeatAttackDelay;
    // The maximum angle that the agent can attack from
    public float attackAngle;

    // The last time the agent attacked
    private float lastAttackTime;
    public StrengthWeakness strengthWeakness;
    RTSPlayer player;
    float upGradeAmount =  1.01f;
    private SimpleObjectPool damageTextObjectPool;
    private GameObject floatingText;

    public override void OnStartAuthority()
    {
        if (NetworkClient.connection.identity == null) { return; }
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        calculatedDamageToDeal = damageToDeal;
        strengthWeakness = GameObject.FindGameObjectWithTag("CombatSystem").GetComponent<StrengthWeakness>();
        damageTextObjectPool = GameObject.FindGameObjectWithTag("DamageTextObjectPool").GetComponent<SimpleObjectPool>();
        //Use this to ensure that the Gizmos are being drawn when in Play Mode.
        m_Started = true;
        originalDamage = damageToDeal;
        DamagePopup.clearText += clearDamageText;
    }

    // Commands are sent from player objects on the client to player objects on the server
    // IF SERVER SIDE , object refernce not found exception
    //

    public void TryAttack()
    {
        //Debug.Log($"Attacker {targeter} attacking .... ");
        Unit unit = GetComponent<Unit>();
        calculatedDamageToDeal = damageToDeal;
        //Use the OverlapBox to detect if there are any other colliders within this box area.
        //Use the GameObject's centre, half the size (as a radius) and rotation. This creates an invisible box around your GameObject.
        Collider[] hitColliders = Physics.OverlapBox(attackPoint.transform.position, transform.localScale * attackRange, Quaternion.identity, layerMask);
        int i = 0;
        Collider other;
        bool isFlipped = false;
        //Check when there is a new collider coming into contact with the box
        while (i < hitColliders.Length)
        {
            other = hitColliders[i++];
            //((RTSNetworkManager)NetworkManager.singleton).Players
            isFlipped = false;
            if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)
            {
                //Debug.Log($"Attack {targeter} , Hit Collider {hitColliders.Length} , Player Tag {targeter.tag} vs Other Tag {other.tag}");
                if ( (other.tag == "Player" + player.GetPlayerID() || other.tag == "King" + player.GetPlayerID() ) && (targeter.tag == "Player" + player.GetPlayerID() || targeter.tag == "King" + player.GetPlayerID())) {continue;}  //check to see if it belongs to the player, if it does, do nothing
                if ( (other.tag == "Player" + player.GetEnemyID() || other.tag == "King" + player.GetEnemyID() ) && (targeter.tag == "Player" + player.GetEnemyID() || targeter.tag == "King" + player.GetEnemyID() ) ) { continue; }  //check to see if it belongs to the player, if it does, do nothing
               
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
                opponentIdentity = (player.GetPlayerID() == 1) ? GetComponent<NetworkIdentity>() : other.GetComponent<NetworkIdentity>();
               
                //Debug.Log($"Original damage {damageToDeal}, {this.GetComponent<Unit>().unitType} , {other.GetComponent<Unit>().unitType} ");
                if (strengthWeakness == null) {
                    strengthWeakness = GameObject.FindGameObjectWithTag("CombatSystem").GetComponent<StrengthWeakness>();
                }
                calculatedDamageToDeal = strengthWeakness.calculateDamage(unit.unitType, other.GetComponent<Unit>().unitType, damageToDeal);
                cmdDamageText(other.transform.position, calculatedDamageToDeal, originalDamage, opponentIdentity, isFlipped);

                if (unit.GetUnitMovement().GetNavMeshAgent().speed == unit.GetUnitMovement().maxSpeed) { calculatedDamageToDeal += 20; }
                //calculatedDamageToDeal += DashDamage;
                if (IsKingSP == true)
                {
                    //GetComponent<KingSP>().IsSuperAttack = false;
                }
                CmdDealDamage(other.gameObject, calculatedDamageToDeal);
                if (IsKingSP == true)
                {
                    
                    cmdCMVirtual();
                    //GetComponent<NavMeshAgent>().speed = GetComponent<UnitMovement>().originalSpeed;
                    ReScaleDamageDeal();
                }
                
                    // DashDamage = 0;
                    //if (targeter.tag.ToLower().Contains("king"))
                    //    Debug.Log($"Strength Weakness damage {calculatedDamageToDeal}");
                    if (unit.unitType == UnitMeta.UnitType.TANK)
                {
                    unit.GetUnitMovement().GetNavMeshAgent().speed = unit.GetUnitMovement().originalSpeed;
                    unit.GetUnitPowerUp().canSpawnEffect = true;
                }
                other.transform.GetComponent<Unit>().GetUnitMovement().CmdTrigger("gethit");
               
                cmdSpecialEffect(other.transform.position);
                if ( UnitMeta.ShakeCamera.ContainsKey (UnitMeta.UnitRaceTypeKey[unit.race][unit.unitType])) { cmdCMVirtual(); }
                //cmdCMFreeLook();
                if(!IsAreaOfEffect)
                    break;
            }

        }

    }
    //Draw the Box Overlap as a gizmo to show where it currently is testing. Click the Gizmos button to see this
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        //Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode
        if (m_Started)
        {
            //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
            //Gizmos.DrawWireCube(attackPoint.transform.position, transform.localScale * attackRange);
        }
    }

    [Command]
    public void CmdDealDamage(GameObject enemy,  float damge)
    {
        //Debug.Log($"attack{damge} DasdhDamage{DashDamage}");
       bool iskill =  enemy.GetComponent<Health>().DealDamage(damge);
        
        if (iskill == true)
        {
            powerUpAfterKill(this.transform.gameObject);
            RpcpowerUpAfterKill(this.transform.gameObject);
            if(TryGetComponent<KingSP>(out KingSP king))
            {
                king.UpdateSPAmount();
            }
            if(IsKingSP == true)
            {
                GetComponent<KingSP>().FindAttackTargetInDistance();
            }
        }
    }
    [Command]
    private void cmdDamageText(Vector3 targetPos, float damageNew, float damgeOld, NetworkIdentity opponentIdentity, bool flipText)
    {

        SetupDamageText(targetPos, damageNew, damgeOld);
        NetworkServer.Spawn(floatingText, connectionToClient);
        
        if (opponentIdentity == null) { return; }
     
        if (flipText) { TargetCommandText(opponentIdentity.connectionToClient, floatingText, opponentIdentity); }
    }
    [Command]
    private void cmdSpecialEffect(Vector3 position)
    {
        GameObject effect = Instantiate(specialEffectPrefab, position, Quaternion.Euler(new Vector3(0, 0, 0)));
        NetworkServer.Spawn(effect, connectionToClient);
    }
    [Command]
    private void cmdCMVirtual()
    {
        if(GameObject.Find("camVirtual") == null) {
            //Debug.Log($" Spawn  camVirtual {GameObject.Find("camVirtual")}");
            //GameObject cam = Instantiate(camPrefab, new Vector2(0,300), Quaternion.Euler(new Vector3(90, 0, 0)));
            GameObject cam = Instantiate(camPrefab, new Vector3(0,0,0), Quaternion.Euler(new Vector3(0, 0, 0)));
            cam.GetComponent<CinemachineShake>().ShakeCamera()  ;
            NetworkServer.Spawn(cam, connectionToClient);
        }
    }
    [Command]
    private void cmdCMFreeLook()
    {
        if (GameObject.Find("camVirtual") == null)
        {
            GameObject cam = Instantiate(camFreeLookPrefab, new Vector3(0, 0, 0), Quaternion.Euler(new Vector3(0, 0, 0)));
            cam.GetComponent<CMFreeLook>().ThirdCamera(GameObject.FindGameObjectWithTag("Player" + player.GetPlayerID()), GameObject.FindGameObjectWithTag("Player" + player.GetEnemyID()));
            NetworkServer.Spawn(cam, connectionToClient);
        }
    }
    public float AttackDistance()
    {
        return attackRange;
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
        //Debug.Log($"unit {targeter.transform.GetComponent<Unit>().name } attacking now, lastAttackTime: {lastAttackTime} ");
        targeter.transform.GetComponent<Unit>().GetUnitMovement().CmdTrigger("attack");
        TryAttack();

    }
    public void ScaleAttackDelay(int factor)
    {
        repeatAttackDelay =  repeatAttackDelay * factor ;
    }
    public void ScaleDamageDeal(float factor)
    {
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
        unit.GetComponent<HealthDisplay>().HandleKillText();
        damageToDeal *= upGradeAmount;
        
    }
    [ClientRpc]
    public void RpcpowerUpAfterKill(GameObject unit)
    {
        unit.GetComponent<HealthDisplay>().HandleKillText();
        damageToDeal *= upGradeAmount;
    }
    [TargetRpc]
    public void TargetCommandText(NetworkConnection other , GameObject floatingText, NetworkIdentity others)
    {
        //Debug.Log("TargetCommandText");
        floatingText.GetComponent<DamageTextHolder>().displayRotation.y = 180; 


    }
    private void Update()
    {
       // if (name == "*KING"&&tag == "King0")
       // {
        //    Debug.Log($"Update DasdhDamage{DashDamage} name --> {name} KingSp = {IsKingSP}");
       // }
        
    }
    private void SetupDamageText(Vector3 targetPos, float damageToDeals, float damageToDealOriginal)
    {
        floatingText = damageTextObjectPool.GetObject();

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
    }
    private void clearDamageText()
    {
        damageTextObjectPool.ReturnObject(floatingText);
    }
}
