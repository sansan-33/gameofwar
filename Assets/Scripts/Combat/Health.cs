using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tactical;
using Mirror;
using UnityEngine;

public class Health : NetworkBehaviour, IDamageable
{
  
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

        DealDamage(currentHealth);
    }

    public void ScaleMaxHealth(float factor)
    {
        maxHealth = (int) (maxHealth * factor);
        currentHealth = maxHealth;
    }
    
    public bool DealDamage(float damageAmount)
    {
        
        if (currentHealth != 0)
        {
            damageAmount -= defense;
            if (damageAmount > 0)
            {
                currentHealth = Mathf.Max(currentHealth - damageAmount, 0);

                if (currentHealth == 0)
                {
                    ServerOnDie?.Invoke(); // if ServerOnDie not null then invoke
                    return true;
                }
               
            }
           
        }

        return false;
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
