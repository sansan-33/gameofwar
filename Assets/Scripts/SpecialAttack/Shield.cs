using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class Shield : NetworkBehaviour
{

    [SerializeField] private GameObject shieldBarParent = null;
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
        startRotation = shieldBarParent.transform.rotation;
    }

    [Command]
    public void CmdSetShieldHealth(int shieldHealth)
    {
        // Debug.Log($"gameobject {this.gameObject.name} {this.shieldHealth} / {shieldHealth}");
        this.shieldHealth = shieldHealth;
        ServerSetShield();
        // NetworkServer.Spawn(ShieldEffect, connectionToClient);//.transform.localScale = new Vector3(5, 5, 5);
    }
    [Server]
    public void ServerSetShield()
    { 
        RpcSetShield();
    }
    [ClientRpc]
    public void RpcSetShield()
    {
        SetShield();
    }
    public void SetShield()
    {
        shield = Instantiate(ShieldEffect, transform);
    }
    private void UpdateShieldHealth(float oldHealth, float newHealth)
    {
        //Debug.Log($"fillamout {ShieldHealthBar.fillAmount},health {shieldHealth}, max {maxShieldHealth}");
        ShieldHealthBar.fillAmount = shieldHealth / maxShieldHealth;
        if(shieldHealth <= 0)
        {
            Destroy(ShieldHealthBar);
            //  Debug.Log($"destroy {ShieldEffect}");
            //Destroy(shield);
            DestroyImmediate(shield, true);

        }
    }
    // Update is called once per frame
    void Update()
    {
        shieldBarParent.transform.rotation = startRotation;
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
    }
}
