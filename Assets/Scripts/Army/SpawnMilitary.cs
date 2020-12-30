using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;

public class SpawnMilitary : NetworkBehaviour
{
    [SerializeField] private GameObject archerPrefab;
    [SerializeField] private GameObject footmanPrefab;
    [SerializeField] private GameObject projectilePrefab = null;
    [SerializeField] private float fireRange = 5;
    
    [SerializeField]
    private float spawnInterval = 60000f;
    private float stoppingDistance = 1;
    private int spawnMoveRange = 1;

    private float chaseRange = 1;
    private int spawnArcherCount=2;
    private int spawnFootmanCount = 2;
    private float lastFireTime;
    [SerializeField] private float fireRate = 6000f;
    private RTSPlayer player;
    public override void OnStartServer()
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        if (FindObjectOfType<NetworkManager>().numPlayers == 1) { 
            while (spawnArcherCount > 0)
            {
                InvokeRepeating("loadArcher", 0.1f, 60000f);
                spawnArcherCount--;
            }
            while (spawnFootmanCount > 0)
            {
                InvokeRepeating("loadFootman", 0.1f, 60000f);
                spawnFootmanCount--;
            }
            InvokeRepeating("TrySlash", 5f, 1f);
            //InvokeRepeating("TryShoot", 5f, 5f);
        }

    }
    private void Update()
    {
        /*
        GameObject target = GameObject.FindGameObjectWithTag("Enemy");
        if(FindObjectOfType<NetworkManager>().numPlayers ==1 && target != null && unit != null)
        {
            if( (target.transform.position - unit.transform.position).magnitude == 0 ) { return; }
            Quaternion targetRotation =
            Quaternion.LookRotation((target.transform.position - unit.transform.position).normalized);

            unit.transform.rotation = Quaternion.RotateTowards(
            unit.transform.rotation, targetRotation, 800 * Time.deltaTime);
        }
        */
    }
    private void loadArcher()
    {

        GameObject unit;
        NavMeshAgent agent = null;

        GameObject[] points = GameObject.FindGameObjectsWithTag("SpawnPoint");
        Vector3 spawnPosition = points[0].transform.position ;
        Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
        spawnOffset.y = spawnPosition.y;
        unit = Instantiate(archerPrefab, spawnPosition + spawnOffset, Quaternion.identity) as GameObject;
        //Debug.Log($"spawnEnemy connectionToClient {player.connectionToClient}");
        unit.GetComponent<Unit>().unitType = Unit.UnitType.ARCHER;
        NetworkServer.Spawn(unit, player.connectionToClient);
            
        agent = unit.GetComponent<NavMeshAgent>();
        agent.speed = 10;
        agent.SetDestination(spawnPosition + spawnOffset);
    }

    private void loadFootman()
    {
        GameObject unit;
        NavMeshAgent agent = null;

        GameObject[] points = GameObject.FindGameObjectsWithTag("SpawnPoint");
        Vector3 spawnPosition = points[0].transform.position;
        Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
        spawnOffset.y = spawnPosition.y;
        unit = Instantiate(footmanPrefab, spawnPosition + spawnOffset, Quaternion.identity) as GameObject;
        //Debug.Log($"spawnEnemy connectionToClient {player.connectionToClient}");
        NetworkServer.Spawn(unit, player.connectionToClient);
        unit.GetComponent<Targeter>().CmdSetAttackType(Targeter.AttackType.Slash);

        unit.GetComponent<Unit>().unitType = Unit.UnitType.SPEARMAN;
        agent = unit.GetComponent<NavMeshAgent>();
        agent.speed = 10;
        agent.SetDestination(spawnPosition + spawnOffset);
    }
   
    private void  TrySlash()
    {

        NavMeshAgent agent = null;
        GameObject target = findNearest("Enemy", 5000);
        GameObject[] armies = GameObject.FindGameObjectsWithTag("Player");
        if (target == null || armies == null) { return; }
        foreach (GameObject army in armies)
        {
            if (army.GetComponent<Unit>().unitType == Unit.UnitType.SPEARMAN)
            {
                army.GetComponent<Targeter>().CmdSetTarget(target, Targeter.AttackType.Slash);
                //army.GetComponent<Targeter>().CmdSetAttackType(Targeter.AttackType.Slash);
                agent = army.GetComponent<NavMeshAgent>();
                agent.speed = 10;
                if (!agent.hasPath) { 
               
                    if ((target.transform.position - army.transform.position).magnitude == 0) { return; }
                    Quaternion targetRotation =
                    Quaternion.LookRotation((target.transform.position - army.transform.position).normalized);
                    army.transform.rotation = Quaternion.RotateTowards(
                    army.transform.rotation, targetRotation, 1000 * Time.deltaTime);
                    agent.SetDestination(target.transform.position);
                    army.GetComponent<UnitWeapon>().Attack();
                    army.GetComponent<Unit>().GetUnitMovement().unitNetworkAnimator.SetTrigger("attack");
                }
                else
                {
                    Debug.Log($"agent has path ? {agent.hasPath}");
                }
            }
        }

    }
    private void TryShoot()
    {
        NavMeshAgent agent = null;

        GameObject target = findNearest("Enemy", 5000);
        GameObject[] armies = GameObject.FindGameObjectsWithTag("Player");
        if(target == null || armies == null) { return; }
        //Debug.Log($"TryShoot 1 armies size {armies.Length} target postion {target}");

        foreach (GameObject army in armies)
        {
            if(army.GetComponent<Unit>().unitType != Unit.UnitType.ARCHER) { return; }
            //Debug.Log($"TryShoot 2 army: {army} at pos: {army.transform.position} , fire range {(target - army.transform.position).sqrMagnitude}");
            army.GetComponent<Targeter>().CmdSetTarget(target, Targeter.AttackType.Shoot);
            if (CanFireAtTarget(target.transform.position, army))
            {
                Quaternion projectileRotation = Quaternion.LookRotation(
                    target.transform.position - army.transform.position);
                GameObject projectileInstance = Instantiate(
                    projectilePrefab, army.transform.Find("ProjectileSpawnPoint").transform.position, projectileRotation);
                //Debug.Log($"TryShoot 3 army {army.transform.position} target {target}");
                NetworkServer.Spawn(projectileInstance, player.connectionToClient);
            }
            else
            {
                agent = army.GetComponent<NavMeshAgent>();
                agent.SetDestination(target.transform.position);
                army.GetComponent<Targeter>().CmdSetTarget(target, Targeter.AttackType.Nothing);
            }
        }
    }
    private bool CanFireAtTarget(Vector3 target, GameObject unit)
    {
        bool canfire = true;
        if ((target - unit.transform.position).sqrMagnitude > fireRange * fireRange)
            canfire = false;
        if (unit.GetComponent<Targeter>().targeterAttackType != Targeter.AttackType.Shoot)
            canfire = false;
        //Debug.Log($"(target - unit.transform.position).sqrMagnitude {(target - unit.transform.position).sqrMagnitude} , fireRange * fireRange {fireRange * fireRange} , canfire ? {canfire}");
        return canfire;
    }

    public GameObject findNearest(string enemyTag, int range)
    {

        GameObject target = null;
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float shortesDistance = Mathf.Infinity;
        Vector3 pos;
        GameObject otherPlayerEnemy = null;
        //Debug.Log($"enemies {enemies.Length}");
        foreach (GameObject enemy in enemies)
        {

            //Debug.Log($"enemy {enemy} / hasAuthority {enemy.GetComponent<Unit>().hasAuthority} , num players : {FindObjectOfType<NetworkManager>().numPlayers }");
            if (FindObjectOfType<NetworkManager>().numPlayers > 1 && enemy.GetComponent<Unit>().hasAuthority) { continue; }
            if (enemy != null && enemy != this.gameObject)
            {
                otherPlayerEnemy = enemy;
                //Debug.Log($"otherPlayerEnemy {otherPlayerEnemy}");
                float distanceToEnemy = Vector3.Distance(transform.position, otherPlayerEnemy.transform.position);
                //targetEnemy = nearestEnemy.GetComponent<Enemy>();
                if (distanceToEnemy < shortesDistance && distanceToEnemy <= range)
                {
                    shortesDistance = distanceToEnemy;
                    target = otherPlayerEnemy;
                }
            }
            //Debug.Log($"target {target} ");
        }
        pos = target.transform.position;
        target.transform.Find("SelectedHighlight").gameObject.GetComponent<SpriteRenderer>().enabled = true;
        target.transform.Find("SelectedHighlight").gameObject.GetComponent<SpriteRenderer>().color = UnityEngine.Random.ColorHSV();

        return target;
    }

}