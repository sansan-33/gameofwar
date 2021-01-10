using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;

public class SpawnMilitary : NetworkBehaviour
{
    [SerializeField] private GameObject archerPrefab;
    [SerializeField] private GameObject footmanPrefab;
    [SerializeField] private GameObject knightPrefab;
    [SerializeField] private GameObject heroPrefab;
    [SerializeField] private GameObject projectilePrefab = null;
    [SerializeField] private float fireRange = 5;
    
    [SerializeField]
    private float spawnInterval = 60000f;
    private float stoppingDistance = 1;
    private int spawnMoveRange = 1;

    private float chaseRange = 1;
    private int spawnArcherCount=4;
    private int spawnFootmanCount = 0;
    private int spawnKnightCount = 0;
    private int spawnHeroCount = 1;
    private float lastFireTime;
    [SerializeField] private float fireRate = 6000f;
    private RTSPlayer player;
    public override void OnStartServer()
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        if (FindObjectOfType<NetworkManager>().numPlayers == 1) { 
            
            StartCoroutine(loadArcher(2f));
            StartCoroutine(loadHero(2f));
            StartCoroutine(loadKnight(2f));
            StartCoroutine(loadFootman(2f));
            //InvokeRepeating("TrySlash", 10f, 2f);
            //InvokeRepeating("TryShoot", 3f, 10f);
        }

    }
    private void Update()
    {
       
    }
    private IEnumerator loadArcher(float waitTime)
    {

        yield return new WaitForSeconds(waitTime);
        while (spawnArcherCount > 0)
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
            unit.GetComponent<Unit>().GetUnitMovement().unitNetworkAnimator.SetTrigger("wait");
            NetworkServer.Spawn(unit, player.connectionToClient);
            
            agent = unit.GetComponent<NavMeshAgent>();
            agent.SetDestination(spawnPosition + spawnOffset);
            spawnArcherCount--;
        }
    }

    private IEnumerator loadFootman(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        while (spawnFootmanCount > 0)
        {
            GameObject unit;
            //NavMeshAgent agent = null;

            GameObject[] points = GameObject.FindGameObjectsWithTag("SpawnPoint");
            Vector3 spawnPosition = points[0].transform.position;
            Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
            spawnOffset.y = spawnPosition.y;
            unit = Instantiate(footmanPrefab, spawnPosition + spawnOffset, Quaternion.identity) as GameObject;
            unit.name = "Footman" + spawnFootmanCount;
            //Debug.Log($"spawnEnemy connectionToClient {player.connectionToClient}");
            //NetworkServer.Spawn(unit, player.connectionToClient);
            //unit.GetComponent<Targeter>().CmdSetAttackType(Targeter.AttackType.Slash);

            //unit.GetComponent<Unit>().unitType = Unit.UnitType.SPEARMAN;
            //unit.GetComponent<Unit>().GetUnitMovement().unitNetworkAnimator.SetTrigger("wait");
            //agent = unit.GetComponent<NavMeshAgent>();
            //agent.SetDestination(points[2].transform.position + spawnOffset);
            spawnFootmanCount--;
        }
    }
    private IEnumerator loadKnight(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        while (spawnKnightCount > 0)
        {
            GameObject unit;
            NavMeshAgent agent = null;

            GameObject[] points = GameObject.FindGameObjectsWithTag("SpawnPoint");
            Vector3 spawnPosition = points[0].transform.position;
            Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
            spawnOffset.y = spawnPosition.y;
            unit = Instantiate(knightPrefab, spawnPosition + spawnOffset, Quaternion.identity) as GameObject;
            unit.name = "Knight" + spawnKnightCount;
            NetworkServer.Spawn(unit, player.connectionToClient);
            unit.GetComponent<Targeter>().CmdSetAttackType(Targeter.AttackType.Slash);

            unit.GetComponent<Unit>().unitType = Unit.UnitType.KNIGHT;
            unit.GetComponent<Unit>().GetUnitMovement().unitNetworkAnimator.SetTrigger("wait");
            agent = unit.GetComponent<NavMeshAgent>();
            agent.SetDestination(spawnPosition + spawnOffset);
            spawnKnightCount--;
        }

    }
    private IEnumerator loadHero(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (spawnHeroCount > 0)
        {
            GameObject unit;
            NavMeshAgent agent = null;

            GameObject[] points = GameObject.FindGameObjectsWithTag("SpawnPoint");
            Vector3 spawnPosition = points[0].transform.position;
            Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
            spawnOffset.y = spawnPosition.y;
            unit = Instantiate(heroPrefab, spawnPosition + spawnOffset, Quaternion.identity) as GameObject;
            unit.name = "Hero";
            NetworkServer.Spawn(unit, player.connectionToClient);
            unit.GetComponent<Targeter>().CmdSetAttackType(Targeter.AttackType.Slash);

            unit.GetComponent<Unit>().unitType = Unit.UnitType.HERO;
            //unit.GetComponent<Unit>().GetUnitMovement().unitNetworkAnimator.SetTrigger("wait");
            agent = unit.GetComponent<NavMeshAgent>();
            agent.SetDestination(spawnPosition + spawnOffset);
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