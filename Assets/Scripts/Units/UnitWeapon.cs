using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class UnitWeapon : NetworkBehaviour
{
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private int damageToDeal = 1;
    [SerializeField] private float destroyAfterSeconds = 1f;
    [SerializeField] private GameObject textPrefab = null;
    [SerializeField] private GameObject projectilePrefab = null;
    [SerializeField] private GameObject camPrefab = null;
    [SerializeField] private GameObject camFreeLookPrefab = null;

    void Start()
    {
    }

    public override void OnStartServer()
    {
        //this.floatingText = tryDamageText(transform.position);
        //Invoke(nameof(DestroySelf), 1);
    }
    [ServerCallback]
    private void Update()
    {
        
       
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other) //sphere collider is used to differentiate between the unit itself, and the attack range (fireRange)
    {

        Targetable target = targeter.GetTarget();
        if (target == null) { return; }
        if (targeter.targeterAttackType != Targeter.AttackType.Slash) { return; }

        Vector3 pos;
        //Debug.Log($"Unit Weapon On Trigger Enter Collide {other}");
        if (other.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity))  //try and get the NetworkIdentity component to see if it's a unit/building 
        {
             //Debug.Log($"same connectionToClient ? other : {networkIdentity.connectionToClient.connectionId}  / this: {connectionToClient.connectionId} / other hasAuthority ? { networkIdentity.hasAuthority}");
            //Debug.Log($"same connectionToClient ?   {networkIdentity.connectionToClient == connectionToClient}");
            
            if (networkIdentity.connectionToClient == connectionToClient && other.tag != "Enemy") { return; }  //check to see if it belongs to the player, if it does, do nothing
            //if (networkIdentity.hasAuthority) { return; }  //check to see if it belongs to the player, if it does, do nothing
        }
        //Debug.Log($"Try Health {other.GetComponent<Health>()}");
        if(other.TryGetComponent<Health>(out Health health))
        {
            pos = other.transform.position;
            health.DealDamage(damageToDeal);
            cmdDamageText(pos);
            //cmdCMVirtual();
            cmdCMFreeLook();
            //Debug.Log($"Deal {damageToDeal} Damage on {other}, totoal health is {health.getCurrentHealth()}");
            // ===================================================================================================
        }
        //DestroySelf();
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
    
    
}
