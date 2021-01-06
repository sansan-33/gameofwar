using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using BehaviorDesigner.Runtime.Tactical;

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

    [ServerCallback]
    private void Update()
    {
        Targetable target = targeter.GetTarget();

        if (target == null) { return; }
        //Debug.Log($"targeter targeterAttackType {targeter.targeterAttackType}");
        if (targeter.targeterAttackType != Targeter.AttackType.Shoot) {return; }
        if (!CanFireAtTarget()) { return; }

        Quaternion targetRotation =
            Quaternion.LookRotation(target.transform.position - transform.position);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (Time.time > (1 / fireRate) + lastFireTime)
        {
            Quaternion projectileRotation = Quaternion.LookRotation(
                target.GetAimAtPoint().position - projectileSpawnPoint.position);


            GameObject projectileInstance = Instantiate(
                projectilePrefab, projectileSpawnPoint.position, projectileRotation);

            //Debug.Log($"Unit Firing projectilePrefab {projectilePrefab} projectileInstance {projectileInstance}");
            //Physics.IgnoreCollision(projectilePrefab.GetComponent<Collider>(), GetComponent<Collider>());


            NetworkServer.Spawn(projectileInstance, connectionToClient);

           
            lastFireTime = Time.time;
        }
    }
    [Server]
    private bool CanFireAtTarget()
    {
        return (targeter.GetTarget().transform.position - transform.position).sqrMagnitude
            <= fireRange * fireRange;
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
        Debug.Log("unit firing now ");
        //Attack();
        lastAttackTime = Time.time;
    }
}
