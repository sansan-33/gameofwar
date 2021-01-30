using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using BehaviorDesigner.Runtime.Tactical;
using System;

public class UnitFiring : NetworkBehaviour, IAttackAgent
{
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private GameObject projectilePrefab = null;
    [SerializeField] private Transform projectileSpawnPoint = null;
    [SerializeField] private float fireRange = 300f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float rotationSpeed = 100f;

    private float lastFireTime;

    // The amount of time it takes for the agent to be able to attack again
    public float repeatAttackDelay;
    // The maximum angle that the agent can attack from
    public float attackAngle;

    // The last time the agent attacked
    private float lastAttackTime;

    [Server]
    private void FireProjectile(Vector3 targetPosition)
    {
        
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        
        Quaternion projectileRotation = Quaternion.LookRotation(targetPosition - projectileSpawnPoint.position);

        GameObject projectileInstance = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileRotation);
        
        NetworkServer.Spawn(projectileInstance, connectionToClient);
            
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

    public float AttackDistance()
    {
        return fireRange;
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
        //Debug.Log("unit firing now ");
        lastAttackTime = Time.time;
        targeter.transform.GetComponent<Unit>().GetUnitMovement().CmdTrigger("attack");
        CmdFireProjectile(targetPosition);
    }
}
