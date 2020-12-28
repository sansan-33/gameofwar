using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;

public class SpawnEnemies : NetworkBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject projectilePrefab = null;
    [SerializeField] private float fireRange = 20;

    private NavMeshAgent agent = null;
    private Camera mainCamera;

    [SerializeField]
    private float spawnInterval = 0.1f;

    private GameObject enemy;
    private float stoppingDistance = 1;
    private float chaseRange = 1;
    private int spawncount=3;
    private float lastFireTime;
    [SerializeField] private float fireRate = 6000f;
    private RTSPlayer player;
    public override void OnStartServer()
    {
        mainCamera = Camera.main;
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        if (FindObjectOfType<NetworkManager>().numPlayers == 1){ 
             while (spawncount > 0)
            {
                InvokeRepeating("SpawnEnemy", 0.1f, this.spawnInterval);
                spawncount--;
            }
        }
    }

    private void Update()
    {
        GameObject target = GameObject.FindGameObjectWithTag("Player");
        if(FindObjectOfType<NetworkManager>().numPlayers ==1 && target != null && enemy != null)
        {
            if( (target.transform.position - enemy.transform.position).magnitude == 0 ) { return; }
            Quaternion targetRotation =
            Quaternion.LookRotation((target.transform.position - enemy.transform.position).normalized);

            enemy.transform.rotation = Quaternion.RotateTowards(
                enemy.transform.rotation, targetRotation, 800 * Time.deltaTime);
                }
        }

    private void SpawnEnemy()
    {

            int i = 0;
            int spawnMoveRange = 1;
            GameObject[] points = GameObject.FindGameObjectsWithTag("SpawnPoint");
            /*
            foreach (GameObject point in points)
            {
                Debug.Log($"12345 SpawnEnemy spawnPosition {i++} {point.transform.position}");
            }
            */
            Vector3 spawnPosition = points[2].transform.position ;
            //Debug.Log($"7777777 SpawnEnemy spawnPosition {spawnPosition}");
            Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
            spawnOffset.y = spawnPosition.y;

            enemy = Instantiate(enemyPrefab, spawnPosition + spawnOffset, Quaternion.identity) as GameObject;
            //Debug.Log($"spawnEnemy connectionToClient {player.connectionToClient}");
            NetworkServer.Spawn(enemy, player.connectionToClient);
            agent = enemy.GetComponent<NavMeshAgent>();
            agent.speed = 10;
            //agent.SetDestination(GameObject.FindGameObjectWithTag("Player").transform.position);

            agent.SetDestination(spawnPosition + spawnOffset);

            //InvokeRepeating("TryMove", 0.1f, 30f);
            InvokeRepeating("TryShoot", 1f, 1f);
    }

    private void  TryMove()
    {
        
        GameObject target = GameObject.FindGameObjectWithTag("Player");
       
        if (target != null && ! agent.hasPath)
        {
            agent.SetDestination(target.transform.position);
            //Debug.Log($"Try Move target.transform.position {target.transform.position}, enemy {enemy.transform.position} , agent.remainingDistance  {agent.remainingDistance }");
        }

    }
    private void TryShoot()
    {
        Vector3 target = findNearest("Player",5000);
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if(target == null || enemies == null) { return; }
        foreach (GameObject enemy in enemies)
        {
            if (!CanFireAtTarget(target,enemy)) { continue; }
            Quaternion projectileRotation = Quaternion.LookRotation(
                     target  - enemy.transform.position);
           GameObject projectileInstance = Instantiate(
                    projectilePrefab, enemy.transform.Find("ProjectileSpawnPoint").transform.position, projectileRotation);

           NetworkServer.Spawn(projectileInstance, player.connectionToClient);
        }

    }
    private bool CanFireAtTarget(Vector3 target, GameObject enemy)
    {
        return (target  - enemy.transform.position).sqrMagnitude
            <= fireRange * fireRange;
    }
    public Vector3 findNearest(string enemyTag, int range)
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
        //pos = mainCamera.WorldToScreenPoint(pos);
        //pos.z = 0.0f;
        target.transform.Find("SelectedHighlight").gameObject.GetComponent<SpriteRenderer>().enabled = true;
        target.transform.Find("SelectedHighlight").gameObject.GetComponent<SpriteRenderer>().color = UnityEngine.Random.ColorHSV();

        return pos;
    }
}