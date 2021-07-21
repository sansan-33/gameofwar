using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using BehaviorDesigner.Runtime.Tactical;
using System;
using System.Linq;

public class UnitFiring : NetworkBehaviour, IAttackAgent, IAttack
{
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private GameObject[] projectilePrefab = null;
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
    public bool AUTOFIRE = false;
    public bool ISCHAINED = false;
    RTSPlayer rtsPlayer;
    // The last time the agent attacked
    private float lastAttackTime;
  

    private void Start()
    {
        UnitProjectile.onKilled += HandleKilled;
        //UnitProjectile.onKilled += RpcOnHandleKilled;
        rtsPlayer = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        
        if (AUTOFIRE) StartCoroutine(autoFire());
    }
    private void OnDestroy()
    {
        UnitProjectile.onKilled -= HandleKilled;
        //UnitProjectile.onKilled -= RpcOnHandleKilled;
    }

    IEnumerator autoFire()
    {
        int targetid = rtsPlayer.GetEnemyID();
        while (true)
        {
            yield return new WaitForSeconds(repeatAttackDelay);
            if (GetComponent<Unit>().unitType == UnitMeta.UnitType.DOOR)
            {
                if (GetComponent<UnitBody>().doorColor != "blue" && GetComponent<UnitBody>().doorColor != "red") { continue; }
                else { targetid = GetComponent<UnitBody>().doorColor == "blue" ? 1 : 0; }
            }
            Attack(ClosestTarget(targetid));
        }
        //yield return null;
    }
    protected Vector3 ClosestTarget(int targetid)
    {
        Transform targetTransform = null ;
        var distance = float.MaxValue;
        var localDistance = 0f;
        GameObject[] units = GameObject.FindGameObjectsWithTag("Player" + targetid);
        GameObject king = GameObject.FindGameObjectWithTag("King" + targetid);
        GameObject[] provokeTanks = GameObject.FindGameObjectsWithTag("Provoke" + targetid);
        GameObject[] sneakyFootman = GameObject.FindGameObjectsWithTag("Sneaky" + targetid);
        List<GameObject> targets = new List<GameObject>();
        targets = units.ToList();
        if (king != null)
            targets.Add(king);
        if (provokeTanks != null && provokeTanks.Length > 0)
            targets.AddRange(provokeTanks.ToList());
        if (sneakyFootman != null && sneakyFootman.Length > 0)
            targets.AddRange(sneakyFootman.ToList());
        //Debug.Log($"AUTO Unit Firing ClosestTarget {targets.Count} ");
        if (targets.Count == 0) { return Vector3.zero; }
        for (int i = targets.Count - 1; i > -1; --i)
        {
            if (targets[i].GetComponent<Health>().IsAlive())
            {
                if ((localDistance = (targets[i].transform.position - this.transform.position).sqrMagnitude) - targets[i].transform.GetComponent<BoxCollider>().size.sqrMagnitude < distance)
                {
                    distance = localDistance;
                    targetTransform = targets[i].transform;
                }
            }
            else
            {
                targets.RemoveAt(i);
            }
        }
        return targetTransform.GetComponent<Targeter>().GetAimAtPoint().position;
    }
    [Server]
    private void HandleFireProjectile(Vector3 targetPosition)
    {
        int arrowIndex = 0;
        if (TryGetComponent(out CardStats cardStats))
            arrowIndex = cardStats.star > 0 ? cardStats.star - 1 : 0;
        GameObject projectile  = arrowIndex >= projectilePrefab.Length ? projectilePrefab[0] : projectilePrefab[arrowIndex];
        Debug.Log($"arrowIndex {arrowIndex} ,projectilePrefab.Length {projectilePrefab.Length} , projectile:  {projectile.name}  ");
        for (var i = 0; i < numShots; i++)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);

            if (GetComponent<Unit>().unitType != UnitMeta.UnitType.DOOR)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        
            Quaternion projectileRotation = Quaternion.LookRotation(targetPosition - projectileSpawnPoint.position);
          
            Vector3 spawnOffset = UnityEngine.Random.insideUnitSphere * spawnMoveRange * numShots;
            spawnOffset.y = 0;
            spawnOffset.z = 0;
            Debug.Log($"Instantiate projectile { projectile.name}");
            GameObject projectileInstance = Instantiate(projectile, projectileSpawnPoint.position + spawnOffset, projectileRotation);
            Debug.Log($"Instantiate projectile { projectile.name}");

            projectileInstance.GetComponent<UnitProjectile>().SetDamageToDeal(damageToDeal, damageToDealFactor);
            var localDistance = (targetPosition - transform.position).sqrMagnitude;
            if (localDistance > 400f)
            projectileInstance.GetComponent<UnitProjectile>().ServerTargetObjectTF(targetPosition);
            if (ISCHAINED) {
                projectileInstance.GetComponent<UnitProjectile>().IS_CHAIN_ATTACK = true;
                projectileInstance.GetComponent<UnitProjectile>().ServerTargetObjectTF(transform.position);
            }
            if(GetComponent<Unit>().unitType == UnitMeta.UnitType.DOOR)
                projectileInstance.GetComponent<UnitProjectile>().SetPlayerType(GetComponent<UnitBody>().doorColor == "blue" ? 0: 1);
            else
                projectileInstance.GetComponent<UnitProjectile>().SetPlayerType(Int32.Parse(tag.Substring(tag.Length - 1)));
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
        HandleFireProjectile(targetPosition);
    }
    public void ChangeAttackDelay(double channgeValue)
    {
        repeatAttackDelay = (float)channgeValue;
    }
    public Transform AttackPoint()
    {
        return projectileSpawnPoint;
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
        return  AUTOFIRE == false && lastAttackTime + repeatAttackDelay < Time.time;
    }

    public float AttackAngle()
    {
        return attackAngle;
    }

    public void Attack(Vector3 targetPosition)
    {
        if (targetPosition == null || targetPosition == Vector3.zero ) { return; }
        UnitAnimator.AnimState animState = UnitAnimator.AnimState.ATTACK;
        lastAttackTime = Time.time;
        var localDistance = (targetPosition - transform.position).sqrMagnitude;
        if (localDistance >= 400f)
            animState = UnitAnimator.AnimState.ATTACK0;
        else if (localDistance < 400f)
            animState = UnitAnimator.AnimState.ATTACK1;
        else if (localDistance < 300f)
            animState = UnitAnimator.AnimState.ATTACK2;

        targeter.transform.GetComponent<UnitAnimator>().StateControl(animState);
        StartCoroutine(FireProjectile(targetPosition));
    }
    IEnumerator FireProjectile(Vector3 targetPosition)
    {
        yield return new WaitForSeconds(0.2f);
        if (GetComponent<Unit>().unitType == UnitMeta.UnitType.DOOR)
            HandleFireProjectile(targetPosition);
        else
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
        damageToDeal = (int)(damageToDeal * factor);
    }
    public void ScaleAttackRange(float _fireRange)
    {
        fireRange = _fireRange;
    }
    public void OnHandleKilled()
    {
        if (UnitMeta.BuildingUnit.Contains(GetComponent<Unit>().unitType)) { return; }
        GetComponent<HealthDisplay>().HandleKillText(1);
        ScaleDamageDeal(0,0,damageToDealFactor + powerUpFactor);
    }
    private void HandleKilled()
    {
        if (isServer)
            RpcOnHandleKilled();
        else
            cmdHandleKilled();
    }
    [Command(requiresAuthority = false)]
    private void cmdHandleKilled()
    {
        ServerHandleKilled();
    }
    [Server]
    private void ServerHandleKilled()
    {
        OnHandleKilled();
    }
    [ClientRpc]
    public void RpcOnHandleKilled()
    {
        if (UnitMeta.BuildingUnit.Contains(GetComponent<Unit>().unitType)) { return; }
        GetComponent<HealthDisplay>().HandleKillText(1);
    }

}
