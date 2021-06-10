using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class Shield : NetworkBehaviour
{

    //[SerializeField] private GameObject shieldBarParent = null;
    [SerializeField] private GameObject healthBarParent = null;
    [SerializeField] public GameObject ShieldEffect;
    [SerializeField] private Image ShieldHealthBar;
    [SyncVar(hook = nameof(UpdateShieldHealth))]
    [HideInInspector] public float shieldHealth = 0;
    private float maxShieldHealth;

    private GameObject shield;
    private bool CanSpawned;
    private Quaternion startRotation;

    private void Awake()
    {
        //startRotation = shieldBarParent.transform.rotation;
    }
    [Command]
    public void CmdSetShield(int shieldHealth, bool IsSpecialButton)
    {
        //Debug.Log($"gameobject {this.gameObject.name} {this.shieldHealth} / {shieldHealth}");
        this.shieldHealth = shieldHealth;
        ServerSetShield(IsSpecialButton);
    }
    [Server]
    public void ServerSetShield(bool IsSpecialButton)
    { 
        RpcSetShield(IsSpecialButton);
    }
    [ClientRpc]
    public void RpcSetShield(bool IsSpecialButton)
    {
        SetShield(IsSpecialButton);
    }
    public void SetShield(bool IsSpecialButton)
    {
        if(shield != null) { Destroy(shield); }
        if (IsSpecialButton)
            shield = Instantiate(ShieldEffect, transform);
        healthBarParent.SetActive(true);
    }
    private void UpdateShieldHealth(float oldHealth, float newHealth)
    {
        //shieldBarParent.SetActive(true);
        //healthBarParent.SetActive(true);
        //Debug.Log($"fillamout {ShieldHealthBar.fillAmount},health {shieldHealth}, max {maxShieldHealth}");
        if (ShieldHealthBar == null || gameObject == null ) { return; }
        ShieldHealthBar.fillAmount = shieldHealth / maxShieldHealth;
        if(shieldHealth <= 0)
        {
            ShieldHealthBar.gameObject.SetActive(false);
            healthBarParent.SetActive(false);
            //Destroy(ShieldHealthBar);
            //  Debug.Log($"destroy {ShieldEffect}");
            //Destroy(shield);
            DestroyImmediate(shield, true);
        }
    }
    // Update is called once per frame
    void Update()
    {
        //shieldBarParent.transform.rotation = startRotation;
        /*
        if (maxShieldHealth == 0 && FindObjectOfType<DefendSP>())
        {
            maxShieldHealth = FindObjectOfType<DefendSP>().shieldHealths;
        }
        if (shieldHealth >= 0)
        {
           
        }
       // Debug.Log($"{shieldHealth}, {CanSpawned}");
        if (shieldHealth > 0 && CanSpawned == true)
        {
            CanSpawned = false;
            
        }
        */
    }
}
