using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tactical;
using Mirror;
using UnityEngine;

public class Health : NetworkBehaviour, IDamageable
{
    [SerializeField] private bool isArcher = false;
    [SerializeField] private bool isFootman = false;
    [SerializeField] private bool isKnight = false;
    [SerializeField] private string ourType = null;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int defense = 0;
    [SyncVar(hook = nameof(HandleHealthUpdated))]
    private int currentHealth;
    
    public event Action ServerOnDie;

    public event Action<int, int> ClientOnHealthUpdated;

    #region Server

    public override void OnStartServer()
    {
        currentHealth = maxHealth;

        UnitBase.ServerOnPlayerDie += ServerHandlePlayerDie;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnPlayerDie -= ServerHandlePlayerDie;
    }

    [Server]
    private void ServerHandlePlayerDie(int connectionId)
    {
        if (connectionToClient.connectionId != connectionId) { return; }

        DealDamage(currentHealth);
    }

    [Server]
    public void isArchers(bool isFootmans, int damageAmount, bool isKnights)
    {
       if(isArcher == true)
        {
            if (isKnights == true )
            {
               
                DamageCalculatesTimes(damageAmount);
            }else if(isFootmans == true)
            {
                DamageCalculatesDivides(damageAmount);
            }
        }
    }
    public void isFootmans(bool isArchers, int damageAmount, bool isKnights)
    {
        if (isFootman == true)
        {
            if (isArchers == true)
            {
                DamageCalculatesTimes(damageAmount);
            }
            else if (isKnights == true)
            {
                DamageCalculatesDivides(damageAmount);
            }
        }
    }
    public void isKnights(bool isArchers, int damageAmount , bool isFootmans)
    {
        if (isKnight == true)
        {
            if (isFootmans == true)
            {
                
                DamageCalculatesTimes(damageAmount);
            }
            else if (isArchers == true)
            {
                DamageCalculatesDivides(damageAmount);
            }
        }
    }
    public void DamageCalculatesTimes(int damageAmount)
    {
        
        damageAmount *= 2;
        DealDamage(damageAmount);
       
    }
    public void DamageCalculatesDivides(int damageAmount)
    {
        damageAmount /= 2;
        DealDamage(damageAmount);
    }
    public void DealDamage(int damageAmount)
    {
      
        if (currentHealth == 0) { return; }
        
        damageAmount -= defense;
        if (damageAmount <= 0) { return; }
        
        currentHealth = Mathf.Max(currentHealth - damageAmount, 0);

        if (currentHealth != 0) { return; }

        ServerOnDie?.Invoke();
    }

    #endregion

    #region Client

    private void HandleHealthUpdated(int oldHealth, int newHealth)
    {
        ClientOnHealthUpdated?.Invoke(newHealth, maxHealth);
    }
    public int getCurrentHealth()
    {
        return currentHealth;
    }

    public void Damage(float amount)
    {
        throw new NotImplementedException();
    }

    public bool IsAlive()
    {
        return true;
    }
    #endregion
}
