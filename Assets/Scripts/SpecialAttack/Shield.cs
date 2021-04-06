using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Shield : NetworkBehaviour
{
    [SerializeField] private ParticleSystem ShieldEffect;
    [SyncVar]
    public float shieldHealth = 0;
    void Start()
    {
        
    }
    [Command]
    public void CmdSetShieldHealth(int shieldHealth)
    {
       // Debug.Log($"gameobject {this.gameObject.name} {this.shieldHealth} / {shieldHealth}");
        this.shieldHealth = shieldHealth;
    }
    // Update is called once per frame
    void Update()
    {
        if(shieldHealth > 0)
        {
            Instantiate(ShieldEffect, this.transform).transform.localScale = new Vector3(5, 5, 5);

        }
    }
}
