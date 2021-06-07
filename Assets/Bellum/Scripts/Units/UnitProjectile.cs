using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tactical;
using Mirror;
using UnityEngine;

public class UnitProjectile : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb = null;
    [SerializeField] private float damageToDeals = 0;
    [SerializeField] private float damageToDealOriginal = 0;
    [SerializeField] private float destroyAfterSeconds = 50f;
    [SerializeField] private float launchForce = 10f;
    [SerializeField] private GameObject camPrefab = null;
    [SerializeField] private string unitType;
    [SerializeField] private GameObject specialEffectPrefab = null;
    [SerializeField] private ElementalDamage.Element element;
    NetworkIdentity opponentIdentity;
    public static event Action onKilled;
    int playerid = 0;
    int enemyid = 0;
    float depth = 1.5f;

    // launch variables
    Vector3 TargetObjectPos = Vector3.zero;
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
            transform.rotation = Quaternion.LookRotation(rb.velocity) * initialRotation;
        }
    }
    public override void OnStartClient()
    {
        if (NetworkClient.connection.identity == null) { return; }
        RTSPlayer player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        playerid = player.GetPlayerID();
        enemyid = player.GetEnemyID();
        initialRotation = rb.rotation;
        Launch();
    }
    void Launch()
    {
        if (TargetObjectPos == Vector3.zero)
        {
            rb.velocity = transform.forward * launchForce;
            return;
        }

        float LaunchAngle = 70f;
        float platformOffset = 0f;
        // think of it as top-down view of vectors: 
        //   we don't care about the y-component(height) of the initial and target position.
        
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
        if (other.tag == "Wall") {
            //Debug.Log($" Hitted object {other.tag}  {other.name}, Attacker arrow type is {unitType} ");
            cmdSpecialEffect(this.transform.position);
            cmdArrowStick(other.transform);
            return;
        }
        // Not attack same connection client object except AI Enemy
        if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1) {
            if ( other.tag.Contains(enemyid.ToString()) && unitType == "Enemy" ) { return; }  //check to see if it belongs to the player, if it does, do nothing
            if ( other.tag.Contains(playerid.ToString()) && unitType == "Player") { return; }  //check to see if it belongs to the player, if it does, do nothing
        }
        else // Multi player seneriao
        {
            isFlipped = true;
            if (this.TryGetComponent<NetworkIdentity>(out NetworkIdentity ArrowNetworkIdentity))
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
            //Debug.Log($"player ID {player.GetPlayerID()}");
            //Debug.Log(playerid);
            
            opponentIdentity = (playerid == 1) ? GetComponent<NetworkIdentity>() : other.GetComponent<NetworkIdentity>();
            //Debug.Log($" Hit Helath Projectile OnTriggerEnter ... {this} , {other.GetComponent<Unit>().unitType} , {damageToDeals}"); 
            //Debug.Log($"before strengthWeakness{damageToDeals}");
            damageToDeals = StrengthWeakness.calculateDamage(UnitMeta.UnitType.ARCHER, other.GetComponent<Unit>().unitType, damageToDeals);
            //Debug.Log("call spawn text");

            //cmdDamageText(other.transform.position, damageToDeals, damageToDealOriginal, opponentIdentity, isFlipped);
            cmdSpecialEffect(other.transform.GetComponent<Unit>().GetTargeter().GetAimAtPoint().position);
            elementalEffect(element, other.transform.GetComponent<Unit>());
            CmdDealDamage(other.gameObject, damageToDeals);
            //Debug.Log($" Hit Helath Projectile OnTriggerEnter ... {this} , {other.GetComponent<Unit>().unitType} , {damageToDeals} / {damageToDealOriginal}");
            cmdArrowStick(other.transform);
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

    [Command]
    public void CmdDealDamage(GameObject enemy, float damge)
    {
        //Debug.Log($"attack{damge} DasdhDamage{DashDamage}");
        if(enemy != null && enemy.TryGetComponent<Health>(out Health health)){
            if(health.DealDamage(damge))
                onKilled?.Invoke();
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

    [Command]
    private void cmdCMVirtual()
    {
        if (GameObject.Find("camVirtual") == null)
        {
            GameObject cam = Instantiate(camPrefab, new Vector2(0, 300), Quaternion.Euler(new Vector3(90, 0, 0)));
            cam.GetComponent<CinemachineShake>().ShakeCamera(0.5f);
            NetworkServer.Spawn(cam, connectionToClient);
        }
    }
    [Command]
    private void cmdSpecialEffect(Vector3 position )
    {
        GameObject effect = Instantiate(specialEffectPrefab, position, Quaternion.Euler(new Vector3(0, 0, 0)));
        NetworkServer.Spawn(effect, connectionToClient);
    }
    [Command]
    private void cmdArrowStick(Transform other)
    {
        arrowStick(other);
    }
    [Server]
    void arrowStick(Transform other)
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
    private void cmdDestroySelf()
    {
        DestroySelf();
    }
    [Server]
    private void DestroySelf()
    {
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
    

}
