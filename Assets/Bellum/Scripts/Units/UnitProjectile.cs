using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime.Tactical;
using Mirror;
using UnityEngine;

public class UnitProjectile : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb = null;
    [SerializeField] private float damageToDeals = 0;
    [SerializeField] private float damageToDealOriginal = 0;
    [SerializeField] private float destroyAfterSeconds = 20f;
    [SerializeField] private float launchForce = 10f;
    [SerializeField] private string unitType;
    [SerializeField] private bool isServerSpawn;
    [SerializeField] private GameObject specialEffectPrefab = null;
    [SerializeField] private ElementalDamage.Element element;
    NetworkIdentity opponentIdentity;
    public static event Action onKilled;
    int playerid = 0;
    int enemyid = 0;
    float depth = 1.5f;

    // launch variables
    Vector3 TargetObjectPos = Vector3.zero;
    Vector3 initialPosition;
    [SyncVar]
    public bool IS_CHAIN_ATTACK = false;
    public bool IS_CHAIN_ENDED = false;

    private bool HITTED = true; // initial state , if hitted = true, will move the hammer to next target
    // state
    private bool bTouchingGround;
    // cache
    private Quaternion initialRotation;

    [SerializeField] private GameObject textPrefab = null;

    // Update is called once per frame
    void Update()
    {
        if (!bTouchingGround && TargetObjectPos != Vector3.zero)
        {
            // update the rotation of the projectile during trajectory motion
            //transform.rotation = Quaternion.LookRotation(rb.velocity) * initialRotation;
            // This will fix wrong enemy arrow rotation 
            transform.rotation = Quaternion.LookRotation(rb.velocity)  ;
        }
    }
    public override void OnStartClient()
    {
        if (NetworkClient.connection.identity == null) { return; }
        RTSPlayer player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        playerid = player.GetPlayerID();
        enemyid = player.GetEnemyID();
        initialRotation = rb.rotation;
        initialPosition = this.transform.position;
        Launch();
    }
    void Launch()
    {
        if (IS_CHAIN_ATTACK)
        {
            StartCoroutine(chainAttack());
            return;  
        }
        if (TargetObjectPos == Vector3.zero)
        {
            rb.velocity = transform.forward * launchForce;
            return;
        }

        float LaunchAngle = 70f;
        float platformOffset = 0f;
        // think of it as top-down view of vectors: 
        // we don't care about the y-component(height) of the initial and target position.
        
        Vector3 projectileXZPos = new Vector3(transform.position.x, transform.position.y , transform.position.z);
        Vector3 targetXZPos = new Vector3(TargetObjectPos.x, transform.position.y, TargetObjectPos.z);

        

        // rotate the object to face the target
        transform.LookAt(targetXZPos);

        // shorthands for the formula
        float R = Vector3.Distance(projectileXZPos, targetXZPos) - 1f;
        float G = Physics.gravity.y;
        float tanAlpha = Mathf.Tan(LaunchAngle * Mathf.Deg2Rad);
        float H = (TargetObjectPos.y + platformOffset) - transform.position.y;

        // calculate the local space components of the velocity 
        // required to land the projectile on the target object 
        float Vz = Mathf.Sqrt(G * R * R / (2.0f * (H - R * tanAlpha)));
        float Vy = tanAlpha * Vz;

        // create the velocity vector in local space and get it in global space
        Vector3 localVelocity = new Vector3(0f, Vy, Vz);
        Vector3 globalVelocity = transform.TransformDirection(localVelocity);

        // launch the object by setting its initial velocity and flipping its state
        if (float.IsNaN(globalVelocity.x) || float.IsNaN(globalVelocity.y) || float.IsNaN(globalVelocity.z))
        {
            rb.velocity = transform.forward * launchForce;
            return;
        }
        rb.velocity = globalVelocity;
        bTouchingGround = false;
        //bTargetReady = false;
    }

    public override void OnStartServer()
    {
        if(!IS_CHAIN_ATTACK)
        Invoke(nameof(DestroySelf), destroyAfterSeconds);
    }
    public void SetDamageToDeal(int damageToDeal , float newDamageToDealFactor)
    {
        //Debug.Log($"damageToDealOriginal {damageToDealOriginal}newDamageToDealFactor{newDamageToDealFactor}");
        this.damageToDeals = damageToDeal == 0 ? this.damageToDeals : damageToDeal;
        damageToDealOriginal = (int) (damageToDealOriginal * newDamageToDealFactor);
    }
    private void OnTriggerEnter(Collider other) //sphere collider is used to differentiate between the unit itself, and the attack range (fireRange)
    {
        bool isFlipped = false;
        bTouchingGround = true;
        damageToDeals = damageToDealOriginal;
        if (other == null || other.name == "Walkable" || other.name == "Door" || other.name.Contains("Trap") ) { return;  }
        if (other.tag.Contains("Building")) {
            //Debug.Log($" Hitted object {other.tag}  {other.name}, Attacker arrow type is {unitType} ");
            specialEffect(this.transform.position);
            arrowStick(other.transform);
            return;
        }
        if (IS_CHAIN_ENDED && other.GetComponent<UnitFiring>() !=null &&  other.GetComponent<UnitFiring>().ISCHAINED )
        {
            if (other.tag.Contains(enemyid.ToString()) && unitType == "Enemy" || other.tag.Contains(playerid.ToString()) && unitType == "Player")
            {
                other.GetComponent<UnitAnimator>().StateControl(UnitAnimator.AnimState.VICTORY);
                DestroySelf();
            }
        }
        
        // Not attack same connection client object except AI Enemy
        if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1) {
            if ( other.tag.Contains(enemyid.ToString()) && unitType == "Enemy" ) { return; }  //check to see if it belongs to the player, if it does, do nothing
            if ( other.tag.Contains(playerid.ToString()) && unitType == "Player") { return; }  //check to see if it belongs to the player, if it does, do nothing
        }
        else // Multi player seneriao
        {
            isFlipped = true;
            if (this.TryGetComponent<NetworkIdentity>(out NetworkIdentity ArrowNetworkIdentity) && !isServerSpawn) // Prevent Checking for Arrow Spawn by Server Object like Door
            {
                if (!ArrowNetworkIdentity.hasAuthority) { return; }
                if (other.TryGetComponent<NetworkIdentity>(out NetworkIdentity OtherNetworkIdentity))  //try and get the NetworkIdentity component to see if it's a unit/building 
                {
                    //Debug.Log($" Hitted object {other.tag} {other.name} hasAuthority {OtherNetworkIdentity.hasAuthority}  OtherNetworkIdentity.connectionToClient: {OtherNetworkIdentity.connectionToClient}  connectionToClient: {connectionToClient} ");
                    if (OtherNetworkIdentity.hasAuthority) { return; }  //check to see if it belongs to the player, if it does, do nothing
                }
             }
        }
        if (other.TryGetComponent<Health>(out Health health))
        {
            //Debug.Log($" Hitted object {other.tag}  {other.name}, Attacker arrow type is {unitType} ");
            //opponentIdentity = (playerid == 1) ? GetComponent<NetworkIdentity>() : other.GetComponent<NetworkIdentity>();
            //Debug.Log($" Hit Helath Projectile OnTriggerEnter ... {this} , {other.GetComponent<Unit>().unitType} , {damageToDeals}"); 
            //Debug.Log($"before strengthWeakness{damageToDeals}");
            damageToDeals = StrengthWeakness.calculateDamage(UnitMeta.UnitType.ARCHER, other.GetComponent<Unit>().unitType, damageToDeals);
           
            //cmdDamageText(other.transform.position, damageToDeals, damageToDealOriginal, opponentIdentity, isFlipped);
            specialEffect(other.transform.GetComponent<Unit>().GetTargeter().GetAimAtPoint().position);
            elementalEffect(element, other.transform.GetComponent<Unit>());
            HITTED = true;
            dealDamage(other.gameObject, damageToDeals,unitType);
            //Debug.Log($" Hit Helath Projectile OnTriggerEnter ... {this} , {other.GetComponent<Unit>().unitType} , {damageToDeals} / {damageToDealOriginal}");
            if(!IS_CHAIN_ATTACK)
                arrowStick(other.transform);
        }
    }
    [Server]
    public void ServerTargetObjectTF(Vector3 targetObjectPos)
    {
        TargetObjectPos = targetObjectPos;
    }
    [Server]
    public void SetPlayerType(int playerid)
    {
        this.unitType = playerid == 0 ? "Player" : "Enemy";
    }
    public void dealDamage(GameObject enemy, float damge, string arrowType)
    {
        if (isServerSpawn)
            ServerDealDamage(enemy, damge, arrowType);
        else
            CmdDealDamage(enemy, damge, arrowType);
    }
    [Command]
    public void CmdDealDamage(GameObject enemy, float damge, string arrowType)
    {
        ServerDealDamage(enemy, damge, arrowType);
    }
    [Server]
    public void ServerDealDamage(GameObject enemy, float damge, string arrowType)
    {
        //Debug.Log($"attack {damge} / enemy {enemy} / arrowType {name} : {arrowType}");
        if (enemy != null && enemy.TryGetComponent<Health>(out Health health))
        {
            if (health.DealDamage(damge))
            {
                onKilled?.Invoke();
                if (enemy.GetComponent<Unit>().unitType == UnitMeta.UnitType.DOOR)
                {
                    //Debug.Log($"Unit Projectile Door Open {arrowType} , set door color {(arrowType == "Enemy" ? "red" : "blue")}");
                    enemy.GetComponent<UnitBody>().SetTeamColor(arrowType == "Enemy" ? "red" : "blue");
                    //GateOpened?.Invoke(arrowType == "Enemy" ? "1" : "0");
                    GreatWallController wallController = GameObject.FindGameObjectWithTag("GreatWallController").GetComponent<GreatWallController>();
                    wallController.GateOpen(arrowType == "Enemy" ? "1" : "0", enemy.GetComponent<UnitBody>().doorIndex.ToString());
                }
            }

        }
    }
    private void elementalEffect(ElementalDamage.Element element, Unit other)
    {
        switch (element)
        {
            case ElementalDamage.Element.ELECTRIC:
                other.GetUnitPowerUp().CmdSpeedUp(-0.5f, true);
                break;
        }
    }
    [Command]
    private void cmdDamageText(Vector3 targetPos, float damageToDeals, float damageToDealOriginal, NetworkIdentity opponentIdentity, bool flipText)
    {
        targetPos.x = targetPos.x + 10;
        targetPos.y = targetPos.y + 5;
        GameObject floatingText = SetupDamageText(targetPos, damageToDeals, damageToDealOriginal);
        NetworkServer.Spawn(floatingText, connectionToClient);
        if (opponentIdentity == null) { return; }
        if (flipText) { TargetCommandText(opponentIdentity.connectionToClient, floatingText, opponentIdentity); }
    }
  
     
    private void specialEffect(Vector3 position )
    {
        /*
        if (isLocalPlayer)
        {
            //            RpcSpecialEffect(position);
            //      else
            CmdSpecialEffect(position);
        }
        */
    }
    [ClientRpc]
    private void RpcSpecialEffect(Vector3 position)
    {
        HandleSpecialEffect(position);
    }
    [Command]
    private void CmdSpecialEffect(Vector3 position)
    {
        ServerSpecialEffect(position);
    }
    [Server]
    private void ServerSpecialEffect(Vector3 position)
    {
        HandleSpecialEffect(position);
    }
    private void HandleSpecialEffect(Vector3 position)
    {
        GameObject effect = Instantiate(specialEffectPrefab, position, Quaternion.Euler(new Vector3(0, 0, 0)));
        NetworkServer.Spawn(effect, connectionToClient);
    }

    private void arrowStick(Transform other)
    {
        if (isServer)
            RpcArrowStick(other);
        else
            CmdArrowStick(other);
        
        //CmdArrowStick(other);
    }
    [ClientRpc]
    private void RpcArrowStick(Transform other)
    {
        HandleArrowStick(other);
    }
    [Command]
    private void CmdArrowStick(Transform other)
    {
        ServerArrowStick(other);
    }
    [Server]
    private void ServerArrowStick(Transform other)
    {
        HandleArrowStick(other);
    }

    void HandleArrowStick(Transform other)
    {
        // move the arrow deep inside the enemy or whatever it sticks to
        transform.Translate(depth * Vector3.forward);
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        if(other != null)
        transform.parent = other.transform.Find("Body");
        //Physics.IgnoreCollision(col.collider, transform.collider);
    }

    [Command]
    private void CmdDestroySelf()
    {
        DestroySelf();
    }
    [Server]
    private void DestroySelf()
    {
        //Debug.Log($"{name} Self destroy after {destroyAfterSeconds}");
        if(gameObject != null)
        NetworkServer.Destroy(gameObject);
    }
    [TargetRpc]
    public void TargetCommandText(NetworkConnection other, GameObject floatingText, NetworkIdentity others)
    {
       // Debug.Log("TargetCommandText");
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
    IEnumerator chainAttack()
    {
        GameObject target;
        GameObject[] units = GameObject.FindGameObjectsWithTag("Player" + enemyid);
        GameObject king = GameObject.FindGameObjectWithTag("King" + enemyid);
        GameObject[] provokeTanks = GameObject.FindGameObjectsWithTag("Provoke" + enemyid);
        GameObject[] sneakyFootman = GameObject.FindGameObjectsWithTag("Sneaky" + enemyid);
        List<GameObject> targets = new List<GameObject>();
        targets = units.ToList();
        if (king != null)
            targets.Add(king);
        if (provokeTanks != null && provokeTanks.Length > 0)
            targets.AddRange(provokeTanks.ToList());
        if (sneakyFootman != null && sneakyFootman.Length > 0)
            targets.AddRange(sneakyFootman.ToList());
        //Debug.Log($"Unit Firing ClosestTarget {targets.Count} ");
        if (targets.Count == 0) { yield break; }

        while (targets.Count > 0) {
            if (!HITTED) {
                yield return new WaitForSeconds(0.5f);
                continue;
            }
            target = ClosestTarget(targets);
            //Debug.Log($"{name} hit target name: {target.name}, total : {targets.Count}");
            transform.LookAt(target.transform);
            rb.velocity = transform.forward * launchForce;
            HITTED = false;
            targets.Remove(target);
            // remove target in targets
        }
        //Debug.Log($"{name} exit while loop for targets total : {targets.Count}");
        transform.LookAt(new Vector3(TargetObjectPos.x, TargetObjectPos.y + 5, TargetObjectPos.z));
        rb.velocity = transform.forward * launchForce;
        IS_CHAIN_ENDED = true;
    }
    protected GameObject ClosestTarget(List<GameObject> _targets)
    {
        Transform targetTransform = null;
        var distance = float.MaxValue;
        var localDistance = 0f;
        
        for (int i = _targets.Count - 1; i > -1; --i)
        {
            if (_targets[i] != null && _targets[i].GetComponent<Health>().IsAlive())
            {
                if ((localDistance = (_targets[i].transform.position - initialPosition).sqrMagnitude) - _targets[i].transform.GetComponent<BoxCollider>().size.sqrMagnitude < distance)
                {
                    distance = localDistance;
                    targetTransform = _targets[i].transform;
                }
            }
            else
            {
                _targets.RemoveAt(i);
            }
        }
        return targetTransform.gameObject;
    }


}
