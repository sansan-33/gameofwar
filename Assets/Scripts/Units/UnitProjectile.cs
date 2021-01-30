using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tactical;
using Mirror;
using UnityEngine;

public class UnitProjectile : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb = null;
    [SerializeField] private int damageToDeals = 0;
    [SerializeField] private float destroyAfterSeconds = 5f;
    [SerializeField] private float launchForce = 10f;
    [SerializeField] private GameObject textPrefab = null;
    [SerializeField] private GameObject camPrefab = null;
    [SerializeField] private string unitType;
    [SerializeField] private GameObject specialEffectPrefab = null;
    private int damageToDealOriginal;
    private StrengthWeakness strengthWeakness;
    RTSPlayer player;

    public override void OnStartClient()
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        damageToDealOriginal += damageToDeals;
        rb.velocity = transform.forward * launchForce;
        strengthWeakness = GameObject.FindGameObjectWithTag("CombatSystem").GetComponent<StrengthWeakness>();
    }

    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), destroyAfterSeconds);
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other) //sphere collider is used to differentiate between the unit itself, and the attack range (fireRange)
    {

        //Debug.Log($" Hitted object {other.tag}, Attacker type is {unitType} ");
        damageToDeals = damageToDealOriginal;
        // Not attack same connection client object except AI Enemy
        if (FindObjectOfType<NetworkManager>().numPlayers == 1) {
            if (other.tag == "Player" + player.GetEnemyID() && unitType == "Enemy" ) { return; }  //check to see if it belongs to the player, if it does, do nothing
            if (other.tag == "Player" + player.GetPlayerID() && unitType == "Player") { return; }  //check to see if it belongs to the player, if it does, do nothing
        }
        else // Multi player seneriao
        {
            if (other.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity))  //try and get the NetworkIdentity component to see if it's a unit/building 
            {
                if (networkIdentity.connectionToClient == connectionToClient) { return; }  //check to see if it belongs to the player, if it does, do nothing
            }

        }
        //Debug.Log($"Health {other} / {other.GetComponent<Health>()} ");
        if (other.TryGetComponent<Health>(out Health health))
        {
            //Debug.Log($" Hit Helath Projectile OnTriggerEnter ... {this} , {other.GetComponent<Unit>().unitType} , {damageToDeals}");
            damageToDeals = strengthWeakness.calculateDamage(Unit.UnitType.ARCHER, other.GetComponent<Unit>().unitType, damageToDeals);
            cmdDamageText(other.transform.position, damageToDeals, damageToDealOriginal);
            cmdSpecialEffect(other.transform.position);
            //if (damageToDeals > damageToDealOriginal) { cmdCMVirtual(); }
            other.transform.GetComponent<Unit>().GetUnitMovement().CmdTrigger("gethit");
            //Debug.Log($" Hit Helath Projectile OnTriggerEnter ... {this} , {other.GetComponent<Unit>().unitType} , {damageToDeals} / {damageToDealOriginal}");
            health.DealDamage(damageToDeals);
        }

        DestroySelf();
    }
    [Command]
    private void cmdDamageText(Vector3 targetPos, int damageToDeals, int damageToDealOriginal)
    {
        GameObject floatingText = Instantiate(textPrefab, targetPos, Quaternion.identity);
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
        NetworkServer.Spawn(effect, connectionToClient);
    }
    [Server]
    private void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
   
}
