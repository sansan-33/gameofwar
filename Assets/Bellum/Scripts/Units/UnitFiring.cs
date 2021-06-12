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
    public bool AUTOFIRE = false;
    RTSPlayer rtsPlayer;
    // The last time the agent attacked
    private float lastAttackTime;
    private void Start()
    {
        UnitProjectile.onKilled += OnHandleKilled;
        UnitProjectile.onKilled += RpcOnHandleKilled;
        rtsPlayer = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        
        if (AUTOFIRE) StartCoroutine(autoFire());
    }
    private void OnDestroy()
    {
        UnitProjectile.onKilled -= OnHandleKilled;
        UnitProjectile.onKilled -= RpcOnHandleKilled;
    }

    IEnumerator autoFire()
    {
        yield return new WaitForSeconds(1f);
        Attack(ClosestTarget());
        yield return null;
    }
    protected Vector3 ClosestTarget()
    {
        Transform targetTransform = null ;
        var distance = float.MaxValue;
        var localDistance = 0f;
        GameObject[] units = GameObject.FindGameObjectsWithTag("Player" + rtsPlayer.GetEnemyID() );
        GameObject king = GameObject.FindGameObjectWithTag("King" + rtsPlayer.GetEnemyID());
        GameObject[] provokeTanks = GameObject.FindGameObjectsWithTag("Provoke" + rtsPlayer.GetEnemyID());
        GameObject[] sneakyFootman = GameObject.FindGameObjectsWithTag("Sneaky" + rtsPlayer.GetEnemyID());
        List<GameObject> targets = new List<GameObject>();
        targets = units.ToList();
        if (king != null)
            targets.Add(king);
        if (provokeTanks != null && provokeTanks.Length > 0)
            targets.AddRange(provokeTanks.ToList());
        if (sneakyFootman != null && sneakyFootman.Length > 0)
            targets.AddRange(sneakyFootman.ToList());
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
        return targetTransform.position;
    }
    [Server]
    private void FireProjectile(Vector3 targetPosition)
    {
        //Debug.Log($"{name} firing now to target position {targetPosition}");
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
            if(fireRange > 20)
            projectileInstance.GetComponent<UnitProjectile>().ServerTargetObjectTF(targetPosition);

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
        FireProjectile(targetPosition);
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
    public void ScaleAttackRange(float _fireRange)
    {
        fireRange = _fireRange;
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
