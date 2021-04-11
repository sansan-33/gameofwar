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
        
        Instantiate(ShieldEffect, transform);
        
    }
    // Update is called once per frame
    void Update()
    {
        if(maxShieldHealth == 0 && FindObjectOfType<DefendSP>())
        {
            maxShieldHealth = FindObjectOfType<DefendSP>().shieldHealths;
        }
        if (shieldHealth > 0)
        {
            ShieldHealthBar.fillAmount = maxShieldHealth / shieldHealth;
        }
        
    }
}
