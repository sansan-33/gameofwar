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
    [SerializeField] private GameObject textPrefab = null;
    [SerializeField] private GameObject camPrefab = null;
    [SerializeField] private string unitType;
    [SerializeField] private GameObject specialEffectPrefab = null;
    NetworkIdentity opponentIdentity;
    public static event Action onKilled;
    private StrengthWeakness strengthWeakness;
    int playerid = 0;
    int enemyid = 0;
    public override void OnStartClient()
    {
        if (NetworkClient.connection.identity == null) { return; }
        RTSPlayer player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        playerid = player.GetPlayerID();
        enemyid = player.GetEnemyID();
        if (strengthWeakness == null) { strengthWeakness = GameObject.FindGameObjectWithTag("CombatSystem").GetComponent<StrengthWeakness>(); }
        //Debug.Log($"damageToDealOriginal {damageToDealOriginal}damageToDeals{damageToDeals}");
        //damageToDealOriginal += damageToDeals;
        //Debug.Log($"damageToDealOriginal after added{damageToDealOriginal}damageToDeals{damageToDeals}");
        rb.velocity = transform.forward * launchForce; 
    }

    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), destroyAfterSeconds);
    }
    public void SetDamageToDeal(float newDamageToDealFactor)
    {
        //Debug.Log($"damageToDealOriginal {damageToDealOriginal}newDamageToDealFactor{newDamageToDealFactor}");
        damageToDealOriginal = (int) (damageToDealOriginal * newDamageToDealFactor);
    }
    [ServerCallback]
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
            if (other.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity))  //try and get the NetworkIdentity component to see if it's a unit/building 
            {
                //Debug.Log($" Hitted object {other.tag} hasAuthority {networkIdentity.hasAuthority} // networkIdentity.connectionToClient: {networkIdentity.connectionToClient}  connectionToClient: {connectionToClient} ");
                //if (networkIdentity.hasAuthority) { return; }  //check to see if it belongs to the player, if it does, do nothing
                if (networkIdentity.connectionToClient == connectionToClient) { return; }  //check to see if it belongs to the player, if it does, do nothing
            }
        }
        //Debug.Log($"Health {other} / {other.GetComponent<Health>()} ");
        if (other.TryGetComponent<Health>(out Health health))
        {
            //Debug.Log($"player ID {player.GetPlayerID()}");
            //Debug.Log(playerid);
            if (playerid == 1)
            {
                opponentIdentity = GetComponent<NetworkIdentity>();
            }
            else
            {
                opponentIdentity = other.GetComponent<NetworkIdentity>();
            }
            //Destroy the arrow faster prevent showing arror after hit
            //gameObject.GetComponent<MeshRenderer>().enabled = false;
            //gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            //Debug.Log($" Hit Helath Projectile OnTriggerEnter ... {this} , {other.GetComponent<Unit>().unitType} , {damageToDeals}"); 
            if (strengthWeakness == null) { strengthWeakness = GameObject.FindGameObjectWithTag("CombatSystem").GetComponent<StrengthWeakness>(); }
            //Debug.Log($"before strengthWeakness{damageToDeals}");
            damageToDeals = strengthWeakness.calculateDamage(UnitMeta.UnitType.ARCHER, other.GetComponent<Unit>().unitType, damageToDeals);
            //Debug.Log("call spawn text");
            cmdDamageText(other.transform.position, damageToDeals, damageToDealOriginal, opponentIdentity, isFlipped);
            cmdSpecialEffect(other.transform.position);
            //if (damageToDeals > damageToDealOriginal) { cmdCMVirtual(); }
            other.transform.GetComponent<Unit>().GetUnitMovement().CmdTrigger("gethit");
            //Debug.Log($"health{health}other{other}");
            //Debug.Log($" Hit Helath Projectile OnTriggerEnter ... {this} , {other.GetComponent<Unit>().unitType} , {damageToDeals} / {damageToDealOriginal}");
            bool iskilled = health.DealDamage(damageToDeals);
            if (iskilled){
                onKilled?.Invoke();
            }
            DestroySelf();
        }
    }
    [Command]
    private void cmdDamageText(Vector3 targetPos, float damageToDeals, float damageToDealOriginal, NetworkIdentity opponentIdentity, bool flipText)
    {
        GameObject floatingText = Instantiate(textPrefab, targetPos, Quaternion.identity);
        //Debug.Log($"spawn {floatingText}");
        Color textColor;
        string dmgText;
        if (damageToDeals > damageToDealOriginal)
        {
            textColor = floatingText.GetComponent<DamageTextHolder>().CriticalColor;
            dmgText = damageToDeals + " Critical";
        } else
        {
            textColor = floatingText.GetComponent<DamageTextHolder>().NormalColor;
            dmgText = damageToDeals + "";
        }
        floatingText.GetComponent<DamageTextHolder>().displayColor = textColor;
        floatingText.GetComponent<DamageTextHolder>().displayText = dmgText;
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
    private void cmdSpecialEffect(Vector3 position)
    {
        GameObject effect = Instantiate(specialEffectPrefab, position, Quaternion.Euler(new Vector3(0, 0, 0)));
        //Debug.Log(effect);
        NetworkServer.Spawn(effect, connectionToClient);
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

}
