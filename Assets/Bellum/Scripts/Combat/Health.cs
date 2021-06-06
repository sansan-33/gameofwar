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
  
    public event Action<int, int, int> ClientOnHealthUpdated;
    public static event Action<GameObject> IceHitUpdated;
    public GameObject specialEffectPrefab;

    [SyncVar] float blinkTimer;
    SkinnedMeshRenderer skinnedMeshRenderer;

    public float blinkDuration;
    public float blinkIntensity;
    HashSet<TacticalAgent> engagedAgentSet = new HashSet<TacticalAgent>();
    Color blinkColor = new Color32 (225,120,120,255);
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
        //else { Destroy(shield.ShieldEffect); }
        if(IsFrezze == true)
        {
            IsFrezze = false;
            //UnFrezze();
        }
        if (currentHealth != 0)
        {
            damageAmount -= defense;
            if (damageAmount > 0)
            {
                currentHealth = Mathf.Max(currentHealth - damageAmount, 0);
                blinkTimer = blinkDuration;
                lastDamageDeal = (int) damageAmount;
                if (currentHealth == 0)
                {
                    StartCoroutine(Die());
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
        var num = ice.enemyList.IndexOf(gameObject);
        ice.enemyList.Remove(gameObject);
        var effect = ice.GetEffect(num);
        effect.GetComponent<RFX4_StartDelay>().Delay = 0;
        GetComponent<UnitPowerUp>().SpecialEffect(ice.GetUnitRepeatAttackDelaykeys(gameObject), ice.GetUnitSpeedkeys(gameObject));

    }
    private IEnumerator Die()
    {
        
        GameObject effect = Instantiate(specialEffectPrefab, transform.position, Quaternion.Euler(new Vector3(0, 0, 0)));
        NetworkServer.Spawn(effect, connectionToClient);
        // Debug.Log($"Destroy unit");
        if (GetComponent<Unit>().unitType == UnitMeta.UnitType.HERO || GetComponent<Unit>().unitType == UnitMeta.UnitType.KING)
        {
            GetComponent<UnitAnimator>().StateControl(UnitAnimator.AnimState.DIE);
            yield return new WaitForSeconds(2.5f);
        }
        ServerOnDie?.Invoke(); // if ServerOnDie not null then invoke
    }
    public void OnElectricShock(float damageAmount,int electricShockDamage)
    {
        DealDamage(damageAmount);
        IsElectricShock = true;
        ElectricDamage = electricShockDamage;
    }
    #endregion

    #region Client
    void start()
    {
        skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
    }
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
        if (blinkTimer > 0)
            BlinkUnit();
            //Invoke("BlinkUnit", 0.1f);
    }
    private void BlinkUnit()
    {
        blinkTimer -= Time.deltaTime;
        float lerp = Mathf.Clamp01(blinkTimer / blinkDuration);
        float intensity = (lerp * blinkIntensity) + 1f;
        intensity = Mathf.Pow(2.0F, intensity);
        if (skinnedMeshRenderer == null) skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        skinnedMeshRenderer.material.SetColor("Color_C33180AA", Color.white * intensity);
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

    public int Engaged(TacticalAgent tacticalAgent)
    {
        if(!engagedAgentSet.Contains(tacticalAgent))
            engagedAgentSet.Add(tacticalAgent);
        //Debug.Log($"Engaged {tacticalAgent},  count {engagedAgentSet.Count}");
        return engagedAgentSet.Count;
    }
    public void Transformhealth()
    {
        currentHealth = maxHealth;
    }

   
    #endregion
}
