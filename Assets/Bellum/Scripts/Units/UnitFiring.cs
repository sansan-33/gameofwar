using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using BehaviorDesigner.Runtime.Tactical;
using System;

public class UnitFiring : NetworkBehaviour, IAttackAgent, IAttack
{
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private GameObject projectilePrefab = null;
    [SerializeField] private Transform projectileSpawnPoint = null;
    [SerializeField] private float fireRange = 300f;
    [SerializeField] private float rotationSpeed = 100f;
   
    private float lastFireTime;
    private int damageToDeal = 0;
    private float damageToDealFactor = 1f;
    private float powerUpFactor = 0.1f;
    private float spawnMoveRange = .5f;

    // The amount of time it takes for the agent to be able to attack again
    public float repeatAttackDelay;
    // The maximum angle that the agent can attack from
    public float attackAngle;
    // The number of arrows per shoot
    public int numShots=1;


    // The last time the agent attacked
    private float lastAttackTime;
    private void Start()
    {
        UnitProjectile.onKilled += OnHandleKilled;
        UnitProjectile.onKilled += RpcOnHandleKilled;
    }
    private void OnDestroy()
    {
        UnitProjectile.onKilled -= OnHandleKilled;
        UnitProjectile.onKilled -= RpcOnHandleKilled;
    }
    [Server]
    private void FireProjectile(Vector3 targetPosition)
    {
        Debug.Log($"{name} firing now to target position {targetPosition}");
        for (var i = 0; i < numShots; i++)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        
            Quaternion projectileRotation = Quaternion.LookRotation(targetPosition - projectileSpawnPoint.position);

            Vector3 spawnOffset = UnityEngine.Random.insideUnitSphere * spawnMoveRange * numShots;
            spawnOffset.y = 0;
            spawnOffset.z = 0;

            GameObject projectileInstance = Instantiate(projectilePrefab, projectileSpawnPoint.position + spawnOffset, projectileRotation);
            projectileInstance.GetComponent<UnitProjectile>().SetDamageToDeal(damageToDeal, damageToDealFactor);
            projectileInstance.GetComponent<UnitProjectile>().ServerTargetObjectTF(targetPosition);

            NetworkServer.Spawn(projectileInstance, connectionToClient);
        }   
    }

    [Server]
    private bool CanFireAtTarget()
    {
        return (targeter.GetTarget().transform.position - transform.position).sqrMagnitude
            <= fireRange * fireRange;
    }
    [Command]
    private void CmdFireProjectile(Vector3 targetPosition)
    {
        FireProjectile(targetPosition);
    }
    public void ChangeAttackDelay(double channgeValue)
    {
        repeatAttackDelay = (float)channgeValue;
    }
    public float AttackDistance()
    {
        return fireRange;
    }
    public float RepeatAttackDelay()
    {
        return repeatAttackDelay;
    }
    public bool CanAttack()
    {
        return lastAttackTime + repeatAttackDelay < Time.time;
    }

    public float AttackAngle()
    {
        return attackAngle;
    }

    public void Attack(Vector3 targetPosition)
    {
        lastAttackTime = Time.time;
        targeter.transform.GetComponent<UnitAnimator>().StateControl(UnitAnimator.AnimState.ATTACK);
        StartCoroutine(FireProjjectile(targetPosition));
    }
    IEnumerator FireProjjectile(Vector3 targetPosition)
    {
        yield return new WaitForSeconds(0.2f);
        CmdFireProjectile(targetPosition);
    }
    public void ScaleAttackDelay(float factor)
    {
        repeatAttackDelay = repeatAttackDelay * factor;
    }
    public void SetNumberOfShoot(int shot)
    {
        numShots = shot;
    }
    public void ScaleDamageDeal(int attack, float repeatAttackDelay, float factor)
    {
        damageToDeal = attack == 0 ? damageToDeal : attack;
        this.repeatAttackDelay = repeatAttackDelay == 0 ? this.repeatAttackDelay : repeatAttackDelay;
        damageToDealFactor = factor;
    }
    public void ScaleAttackRange(float factor)
    {
        fireRange = fireRange * factor;
    }
    public void OnHandleKilled()
    {
        GetComponent<HealthDisplay>().HandleKillText(1);
        ScaleDamageDeal(0,0,damageToDealFactor + powerUpFactor);
    }
    [ClientRpc]
    public void RpcOnHandleKilled()
    {
        GetComponent<HealthDisplay>().HandleKillText(1);
    }

}
