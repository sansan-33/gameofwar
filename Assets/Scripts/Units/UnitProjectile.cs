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
    [SerializeField] private float destroyAfterSeconds = 5f;
    [SerializeField] private float launchForce = 10f;
    [SerializeField] private GameObject camPrefab = null;
    [SerializeField] private string unitType;
    [SerializeField] private GameObject specialEffectPrefab = null;
    [SerializeField] private ElementalDamage.Element element;
    NetworkIdentity opponentIdentity;
    public static event Action onKilled;
    int playerid = 0;
    int enemyid = 0;
    [SerializeField] private GameObject textPrefab = null;

    public override void OnStartClient()
    {
        if (NetworkClient.connection.identity == null) { return; }
        RTSPlayer player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        playerid = player.GetPlayerID();
        enemyid = player.GetEnemyID();
        rb.velocity = transform.forward * launchForce;
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
        //Debug.Log($" Hitted object {other.tag}  {other.name}, Attacker arrow type is {unitType} ");
        damageToDeals = damageToDealOriginal;
        // Not attack same connection client object except AI Enemy
        if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1) {
            if ((other.tag == "Player" + enemyid || other.tag == "King" + enemyid) && unitType == "Enemy" ) { return; }  //check to see if it belongs to the player, if it does, do nothing
            if ((other.tag == "Player" + playerid || other.tag == "King" + playerid ) && unitType == "Player") { return; }  //check to see if it belongs to the player, if it does, do nothing
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
        //Debug.Log($"Health hitted {other.name} {other.tag} / arrow type {unitType } / {other.GetComponent<Health>()} ");
        if (other.TryGetComponent<Health>(out Health health))
        {
            //Debug.Log($"player ID {player.GetPlayerID()}");
            //Debug.Log(playerid);
            opponentIdentity = (playerid == 1) ? GetComponent<NetworkIdentity>() : other.GetComponent<NetworkIdentity>();
            //Debug.Log($" Hit Helath Projectile OnTriggerEnter ... {this} , {other.GetComponent<Unit>().unitType} , {damageToDeals}"); 
            //Debug.Log($"before strengthWeakness{damageToDeals}");
            damageToDeals = StrengthWeakness.calculateDamage(UnitMeta.UnitType.ARCHER, other.GetComponent<Unit>().unitType, damageToDeals);
            //Debug.Log("call spawn text");
            
            cmdDamageText(other.transform.position, damageToDeals, damageToDealOriginal, opponentIdentity, isFlipped);
            cmdSpecialEffect(other.transform.GetComponent<Unit>().GetTargeter().GetAimAtPoint().position);
            elementalEffect(element, other.transform.GetComponent<Unit>());
            //if (damageToDeals > damageToDealOriginal) { cmdCMVirtual(); }
            //other.transform.GetComponent<Unit>().GetUnitMovement().CmdTrigger("gethit");
            CmdDealDamage(other.gameObject, damageToDeals);
            //Debug.Log($" Hit Helath Projectile OnTriggerEnter ... {this} , {other.GetComponent<Unit>().unitType} , {damageToDeals} / {damageToDealOriginal}");
            cmdDestroySelf();
        }
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
                other.GetUnitPowerUp().cmdSpeedUp(-1);
                break;
        }
    }
    [Command]
    private void cmdDamageText(Vector3 targetPos, float damageToDeals, float damageToDealOriginal, NetworkIdentity opponentIdentity, bool flipText)
    {
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
            cam.GetComponent<CinemachineShake>().ShakeCamera();
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
    private void cmdDestroySelf()
    {
        DestroySelf();
    }
    [Server]
    private void DestroySelf()
    {
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
