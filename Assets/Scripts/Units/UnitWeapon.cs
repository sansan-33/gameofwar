using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tactical;
using Mirror;
using TMPro;
using UnityEngine;

public class UnitWeapon : NetworkBehaviour, IAttackAgent, IAttack
{
   
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private int damageToDeal = 1;
    [SerializeField] private float destroyAfterSeconds = 1f;
    [SerializeField] private GameObject textPrefab = null;
    [SerializeField] private GameObject camPrefab = null;
    [SerializeField] private GameObject camFreeLookPrefab = null;
    [SerializeField] private GameObject attackPoint;
    [SerializeField] private float attackRange=5f;
    [SerializeField] private LayerMask layerMask = new LayerMask();
    [SerializeField] private GameObject specialEffectPrefab  = null;
    [SerializeField] private bool IsAreaOfEffect = false;

    private int id;
    private int calculatedDamageToDeal ;
    
    bool m_Started;
    // The amount of time it takes for the agent to be able to attack again
    public float repeatAttackDelay;
    // The maximum angle that the agent can attack from
    public float attackAngle;

    // The last time the agent attacked
    private float lastAttackTime;
    public StrengthWeakness strengthWeakness;
    RTSPlayer player;

    public override void OnStartAuthority()
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        calculatedDamageToDeal = damageToDeal;
        //lastAttackTime = -repeatAttackDelay;
        strengthWeakness = GameObject.FindGameObjectWithTag("CombatSystem").GetComponent<StrengthWeakness>();
        //Debug.Log($"Is strengthWeakness is null ? {strengthWeakness == null}");
        //Use this to ensure that the Gizmos are being drawn when in Play Mode.
        m_Started = true;
    }
    
    [Command]
    public void TryAttack()
    {
        //Debug.Log($"Attacker {targeter} attacking .... ");

        calculatedDamageToDeal = damageToDeal;
        //Use the OverlapBox to detect if there are any other colliders within this box area.
        //Use the GameObject's centre, half the size (as a radius) and rotation. This creates an invisible box around your GameObject.
        Collider[] hitColliders = Physics.OverlapBox(attackPoint.transform.position, transform.localScale * attackRange, Quaternion.identity, layerMask);
        int i = 0;
        Collider other;
        //Check when there is a new collider coming into contact with the box
        while (i < hitColliders.Length)
        {
            other = hitColliders[i++];
               
            if (FindObjectOfType<NetworkManager>().numPlayers == 1)
            {
                //Debug.Log($"Attack {targeter} , Hit Collider {hitColliders.Length} , Player Tag {targeter.tag} vs Other Tag {other.tag}");
                if (other.tag == "Player" + player.GetPlayerID() && targeter.tag == "Player" + player.GetPlayerID()) {continue;}  //check to see if it belongs to the player, if it does, do nothing
                if (other.tag == "Player" + player.GetEnemyID() && targeter.tag == "Player" + player.GetEnemyID()) { continue; }  //check to see if it belongs to the player, if it does, do nothing
                
            }
            else // Multi player seneriao
            {
                if (other.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity))  //try and get the NetworkIdentity component to see if it's a unit/building 
                {
                    if (networkIdentity.connectionToClient == connectionToClient) { continue; }  //check to see if it belongs to the player, if it does, do nothing
                }
            }
            //Debug.Log($"Attacker {targeter} --> Enemy {other} tag {other.tag}");

            if (other.TryGetComponent<Health>(out Health health))
            {
                //Debug.Log($"Original damage {damageToDeal}, {this.GetComponent<Unit>().unitType} , {other.GetComponent<Unit>().unitType} ");
                if(strengthWeakness == null) {
                    strengthWeakness = GameObject.FindGameObjectWithTag("CombatSystem").GetComponent<StrengthWeakness>();
                }
                calculatedDamageToDeal = strengthWeakness.calculateDamage(this.GetComponent<Unit>().unitType, other.GetComponent<Unit>().unitType, damageToDeal);
                health.DealDamage(calculatedDamageToDeal);
                if(calculatedDamageToDeal == 0)
                    Debug.Log($"Damage is ZERO {this.GetComponent<Unit>().unitType} vs {other.GetComponent<Unit>().unitType}  = damage: {damageToDeal} /  {calculatedDamageToDeal}");
                other.transform.GetComponent<Unit>().GetUnitMovement().CmdTrigger("gethit");
                cmdDamageText(other.transform.position, calculatedDamageToDeal , damageToDeal );
                cmdSpecialEffect(other.transform.position);
                //if (calculatedDamageToDeal > damageToDeal ) { cmdCMVirtual(); }
                //cmdCMFreeLook();
                if(!IsAreaOfEffect)
                    break;
            }

        }

    }
    public int Getids()
    {
        return id;
    }
    //Draw the Box Overlap as a gizmo to show where it currently is testing. Click the Gizmos button to see this
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        //Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode
        if (m_Started)
        {
            //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
            Gizmos.DrawWireCube(attackPoint.transform.position, transform.localScale * attackRange);
        }
    }


    [Command]   
    private void cmdDamageText(Vector3 targetPos, int damageNew , int damgeOld)
    {
        GameObject floatingText = Instantiate(textPrefab, targetPos, Quaternion.identity);
        Color textColor;
        string dmgText;
        if (damageNew > damgeOld)
        {
            textColor = floatingText.GetComponent<DamageTextHolder>().CriticalColor;
            dmgText = damageNew + " Critical";
        }
        else
        {
            textColor = floatingText.GetComponent<DamageTextHolder>().NormalColor;
            dmgText = damageNew + "";
        }
        floatingText.GetComponent<DamageTextHolder>().displayColor = textColor;
        floatingText.GetComponent<DamageTextHolder>().displayText = dmgText;
        NetworkServer.Spawn(floatingText, connectionToClient);


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
        Debug.Log($"Scale Damage {this.GetComponent<Unit>().unitType} {damageToDeal} {factor} ");
        damageToDeal =  (int)  (damageToDeal * factor);
    }
}
