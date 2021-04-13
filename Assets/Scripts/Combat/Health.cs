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
    [SerializeField] private Shield shield;
    [SyncVar(hook = nameof(HandleHealthUpdated))]
    private float currentHealth;
    [SyncVar]
    private int currentLevel;
    private int lastDamageDeal;
    private int ElectricDamage;
    private float electricTimer = 1;
    public bool IsFrezze = false;
    public bool IsElectricShock = false;
    public event Action ServerOnDie;
    public event Action ClientOnDie;

    public event Action<int, int, int> ClientOnHealthUpdated;
    public static event Action<GameObject> IceHitUpdated;

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
    public void SetUnitLevel(int level)
    {
        this.currentLevel = level;
    }
    public int GetUnitLevel()
    {
        return this.currentLevel;
    }
    public void ScaleMaxHealth(int health, float factor)
    {
        maxHealth = health == 0 ? maxHealth : health;
        maxHealth = (int) (maxHealth * factor);
        currentHealth = maxHealth;
    }
    public void Healing(float healAmount)
    {
        if (currentHealth >= maxHealth) { return; }
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
    }
    public bool DealDamage(float damageAmount)
    {
        if (shield.shieldHealth > 0)
        {
            //Debug.Log($"attack shield health is --> {shield.shieldHealth}");
            shield.shieldHealth -= damageAmount;
            return false;
        }
        else { Destroy(shield.ShieldEffect); }
        if(IsFrezze == true)
        {
            IsFrezze = false;
            UnFrezze();
        }
        if (currentHealth != 0)
        {
            damageAmount -= defense;
            if (damageAmount > 0)
            {
                currentHealth = Mathf.Max(currentHealth - damageAmount, 0);
                lastDamageDeal = (int) damageAmount;
                if (currentHealth == 0)
                {
                    ServerOnDie?.Invoke(); // if ServerOnDie not null then invoke
                    ClientOnDie?.Invoke();
                    //StartCoroutine(Die());
                    return true;
                }
            }
        }
        return false;
    }
    public void UnFrezze()
    {
        Debug.Log("Unfrezze");
        Ice ice = FindObjectOfType<Ice>();
        ice.enemyList.Remove(gameObject);
        CardStats cardStats = GetComponent<CardStats>();
        GetComponent<UnitPowerUp>().CmdPowerUp(gameObject, cardStats.star, cardStats.cardLevel, cardStats.health, cardStats.attack, ice.GetUnitRepeatAttackDelaykeys(gameObject), ice.GetUnitSpeedkeys(gameObject), cardStats.defense, cardStats.special);

    }
    private IEnumerator Die()
    {
        GetComponent<UnitMovement>().HandleDieAnnimation();
        CardStats cardStats = GetComponent<CardStats>();
        GetComponent<UnitPowerUp>().CmdPowerUp(gameObject, cardStats.star, cardStats.cardLevel, cardStats.health, cardStats.attack, Mathf.Infinity, 0, cardStats.defense, cardStats.special);

        yield return new WaitForSeconds(5);
        ServerOnDie?.Invoke(); // if ServerOnDie not null then invoke
        ClientOnDie?.Invoke();
    }
    public void OnElectricShock(float damageAmount,int electricShockDamage)
    {
        DealDamage(damageAmount);
        IsElectricShock = true;
        ElectricDamage = electricShockDamage;
    }
    #endregion

    #region Client
    private void Update()
    {
        if (IsElectricShock&& electricTimer > 0)
        {
            electricTimer -= Time.deltaTime;
        }
        else if(IsElectricShock)
        {
            electricTimer = 1;
            DealDamage(ElectricDamage);
        }
    }
    private void HandleHealthUpdated(float oldHealth, float newHealth)
    {
        ClientOnHealthUpdated?.Invoke((int)newHealth, (int)maxHealth, lastDamageDeal);
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
    public void Transformhealth()
    {
        currentHealth = maxHealth;
    }
    #endregion
}
