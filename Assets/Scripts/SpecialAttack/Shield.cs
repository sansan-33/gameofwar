using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class Shield : NetworkBehaviour
{
    [SerializeField] public GameObject ShieldEffect;
    [SerializeField] private Image ShieldHealthBar;
    [SyncVar]
    public float shieldHealth = 0;
    private float maxShieldHealth;
    private bool CanSpawned = true;
    void Start()
    {
        
    }
    [Command]
    public void CmdSetShieldHealth(int shieldHealth)
    {
        // Debug.Log($"gameobject {this.gameObject.name} {this.shieldHealth} / {shieldHealth}");
        CanSpawned = true;
        this.shieldHealth = shieldHealth;
        //
        ServerSetShield(ShieldEffect);
        // NetworkServer.Spawn(ShieldEffect, connectionToClient);//.transform.localScale = new Vector3(5, 5, 5);
    }
    private void ServerSetShield(GameObject ShieldEffect)
    {
        // Will not work at double player
        Instantiate(ShieldEffect, transform);
    }
    // Update is called once per frame
    void Update()
    {
        if (maxShieldHealth == 0 && FindObjectOfType<DefendSP>())
        {
            maxShieldHealth = FindObjectOfType<DefendSP>().shieldHealths;
        }
        if (shieldHealth >= 0)
        {
            ShieldHealthBar.fillAmount = shieldHealth / maxShieldHealth;
        }
        if (shieldHealth > 0 && CanSpawned == true)
        {
            CanSpawned = false;
            
        }
        
        
    }
}
