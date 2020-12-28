using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;


public class UnitAttack : NetworkBehaviour
{
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private GameObject swordPrefab = null;
    [SerializeField] private Transform projectileSpawnPoint = null;
    [SerializeField] private float attackRange = 150f;
    [SerializeField] private float rotationSpeed = 20f;
    [SerializeField] private float fireRate = 1f;

    private float lastFireTime;
    private bool isEnabled = false;

    [ServerCallback]
    private void Update()
    {
        Targetable target = targeter.GetTarget();

        if (target == null) { return; }

        if (!CanFireAtTarget()) { return; }

        if (Time.time > (1 / fireRate) + lastFireTime && isEnabled)
        {
            Quaternion targetRotation =
            Quaternion.LookRotation(target.transform.position - transform.position);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        Quaternion projectileRotation = Quaternion.LookRotation(
                target.GetAimAtPoint().position - projectileSpawnPoint.position);

            GameObject swordInstance = Instantiate(swordPrefab, projectileSpawnPoint.transform);
            
                        NetworkServer.Spawn(swordPrefab, connectionToClient);
            
            swordInstance.transform.parent = transform;
            lastFireTime = Time.time;
        }

    }
    [Server]
    private bool CanFireAtTarget()
    {
        return (targeter.GetTarget().transform.position - transform.position).sqrMagnitude
            <= attackRange * attackRange;
    }

   
}
