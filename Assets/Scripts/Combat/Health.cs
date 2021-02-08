using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tactical;
using Mirror;
using UnityEngine;

public class Health : NetworkBehaviour, IDamageable
{
    [SerializeField] private string ourType = null;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int defense = 0;
    [SyncVar(hook = nameof(HandleHealthUpdated))]
    private float currentHealth;
    
    public event Action ServerOnDie;

    public event Action<int, int> ClientOnHealthUpdated;

    #region Server

    public override void OnStartServer()
    {
        currentHealth = maxHealth;

        UnitBase.ServerOnPlayerDie += ServerHandlePlayerDie;  // subscribe the the event
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnPlayerDie -= ServerHandlePlayerDie; // unsubscribe the the event
    }

    [Server]
    private void ServerHandlePlayerDie(int connectionId)
    {
        if (connectionToClient.connectionId != connectionId) { return; }

        DealDamage(currentHealth,this.GetComponent<Unit>(),3);
    }

    public void ScaleMaxHealth(float factor)
    {
        maxHealth = (int) (maxHealth * factor);
        currentHealth = maxHealth;
    }
    
    public void DealDamage(float damageAmount,Unit unit,int IsUnitWeapon)
    {
      
        if (currentHealth == 0) { return; }
        
        damageAmount -= defense;
        if (damageAmount <= 0) { return; }
        
        currentHealth = Mathf.Max(currentHealth - damageAmount, 0);

        if (currentHealth != 0) { return; }
        if (IsUnitWeapon == 0)
        {
            unit.GetComponent<UnitWeapon>().powerUpAfterKill();
        }else if(IsUnitWeapon == 1)
        {
            unit.GetComponent<UnitProjectile>().powerUpAfterKill();
        }
        
        ServerOnDie?.Invoke(); // if ServerOnDie not null then invoke 
    }

    #endregion

    #region Client

    private void HandleHealthUpdated(float oldHealth, float newHealth)
    {
        ClientOnHealthUpdated?.Invoke((int)newHealth, (int)maxHealth);
    }
    public float getCurrentHealth()
    {
        return currentHealth;
    }

    public void Damage(float amount)
    {
        throw new NotImplementedException();
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }
    #endregion
}
