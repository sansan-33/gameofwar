using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tactical;
using Mirror;
using TMPro;
using UnityEngine;

public class UnitWeapon : NetworkBehaviour, IAttackAgent
{
    [SerializeField] private bool isArcher = false;
    [SerializeField] private bool isFootman = false;
    [SerializeField] private bool isKnight = false;
    [SerializeField] private Unit units = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private int damageToDeal = 1;
    [SerializeField] private float destroyAfterSeconds = 1f;
    [SerializeField] private GameObject textPrefab = null;
    [SerializeField] private GameObject camPrefab = null;
    [SerializeField] private GameObject camFreeLookPrefab = null;
    [SerializeField] private GameObject attackPoint;
    [SerializeField] private float attackRange=5f;
    [SerializeField] private LayerMask layerMask = new LayerMask();
    private int id;
    bool m_Started;
    // The amount of time it takes for the agent to be able to attack again
    public float repeatAttackDelay;
    // The maximum angle that the agent can attack from
    public float attackAngle;

    // The last time the agent attacked
    private float lastAttackTime;
    private StrengthWeakness strengthWeakness;

    void Start()
    {
        lastAttackTime = -repeatAttackDelay;
        strengthWeakness = GameObject.FindGameObjectWithTag("CombatSystem").GetComponent<StrengthWeakness>();

    }

    public override void OnStartServer()
    {
        //Use this to ensure that the Gizmos are being drawn when in Play Mode.
        m_Started = true;
    }
    [ServerCallback]
    private void Update()
    {
        
       
    }

    [Command]
    public void Attack()
    {
     
        //Use the OverlapBox to detect if there are any other colliders within this box area.
        //Use the GameObject's centre, half the size (as a radius) and rotation. This creates an invisible box around your GameObject.
        Collider[] hitColliders = Physics.OverlapBox(attackPoint.transform.position, transform.localScale * attackRange, Quaternion.identity, layerMask);
        int i = 0;
        Collider other;
        //Check when there is a new collider coming into contact with the box
        while (i < hitColliders.Length)
        {
            other = hitColliders[i];
            if (other.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity))  //try and get the NetworkIdentity component to see if it's a unit/building 
            {
                if (networkIdentity.connectionToClient == connectionToClient && other.tag != "Enemy") { return; }  //check to see if it belongs to the player, if it does, do nothing
            }
            if (other.TryGetComponent<Health>(out Health health))
            {
                //Debug.Log(true);
                Debug.Log($"Original damage {damageToDeal}, {this.GetComponent<Unit>().unitType} , {other.GetComponent<Unit>().unitType} ");
                damageToDeal = strengthWeakness.calculateDamage(this.GetComponent<Unit>().unitType, other.GetComponent<Unit>().unitType, damageToDeal);
                health.DealDamage(damageToDeal);
                Debug.Log($"Strength Weakness damage {damageToDeal}");


                //health.isFootmans(isKnight, damageToDeal, isArcher);
                //health.isKnights(isArcher, damageToDeal, isFootman);
                //health.isArchers(isFootman, damageToDeal, isKnight);
                cmdDamageText(other.transform.position);
                cmdCMVirtual();
                break;
            }
            i++;
        }

    }
    public int Getids()
    {
        return id;
    }
    //Draw the Box Overlap as a gizmo to show where it currently is testing. Click the Gizmos button to see this
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode
        if (m_Started)
        {
            //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
            Gizmos.DrawWireCube(attackPoint.transform.position, transform.localScale);
        }
    }


    [Command]   
    private void cmdDamageText(Vector3 targetPos)
    {
        GameObject floatingText = Instantiate(textPrefab, targetPos, Quaternion.identity);
        floatingText.GetComponent<DamageTextHolder>().displayColor = Color.blue;
        floatingText.GetComponent<DamageTextHolder>().displayText = damageToDeal + "";
        NetworkServer.Spawn(floatingText, connectionToClient);
    }
    [Command]
    private void cmdCMVirtual()
    {
        if(GameObject.Find("camVirtual") == null) {
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
            cam.GetComponent<CMFreeLook>().ThirdCamera(GameObject.FindGameObjectWithTag("Player"), GameObject.FindGameObjectWithTag("Enemy"));
            NetworkServer.Spawn(cam, connectionToClient);
        }
    }
    [Server]
    private void DestroySelf()
    {
        Destroy(gameObject);
        //NetworkServer.Destroy(gameObject);
    }

    public float AttackDistance()
    {
        return attackRange;
    }

    public bool CanAttack()
    {
        return lastAttackTime + repeatAttackDelay < Time.time;
    }

    public float AttackAngle()
    {
        return attackAngle;
    }

    public void Attack(Vector3 targetPosition)
    {
        Debug.Log("unit weapon attacking now ");
        Attack();
        lastAttackTime = Time.time;
    }
}
