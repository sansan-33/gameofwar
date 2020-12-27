using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;

public class SpawnEnemies : NetworkBehaviour
{
    [SerializeField]
    private GameObject enemyPrefab;
    [SerializeField] private GameObject projectilePrefab = null;
    private NavMeshAgent agent = null;

    [SerializeField]
    private float spawnInterval = 300.0f;

    private GameObject enemy;
    private float stoppingDistance = 1;
    private float chaseRange = 1;
    private float lastFireTime;
    [SerializeField] private float fireRate = 6000f;
    private RTSPlayer player;
    public override void OnStartServer()
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        if(FindObjectOfType<NetworkManager>().numPlayers ==1)
            InvokeRepeating("SpawnEnemy", 0.1f, this.spawnInterval);
    }

    private void Update()
    {
        GameObject target = GameObject.FindGameObjectWithTag("Player");
        if(FindObjectOfType<NetworkManager>().numPlayers ==1 && target != null && enemy != null)
        {
            Quaternion targetRotation =
            Quaternion.LookRotation((target.transform.position - enemy.transform.position).normalized);

            enemy.transform.rotation = Quaternion.RotateTowards(
                enemy.transform.rotation, targetRotation, 800 * Time.deltaTime);
                }
        }

    private void SpawnEnemy()
    {

        //if (GameObject.FindGameObjectWithTag("Enemy") != null ) { return; }
        //if (Time.time > (1 / 0.01) + lastFireTime)
        //{
            //RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();
            
            Vector2 spawnPosition = new Vector2(Random.Range(-4.0f, 4.0f), this.transform.position.y);
            enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity) as GameObject;
            agent = enemy.GetComponent<NavMeshAgent>();
            agent.speed = 10;
            agent.SetDestination(GameObject.FindGameObjectWithTag("Player").transform.position);
            //Debug.Log($"spawnEnemy connectionToClient {player.connectionToClient}");
            NetworkServer.Spawn(enemy, player.connectionToClient);
            InvokeRepeating("TryMove", 0.1f, 30f);
            InvokeRepeating("TryShoot", 0.1f, 2f);
            //lastFireTime = Time.time;
            //Destroy(enemy, 10);
        //}
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
        GameObject target = GameObject.FindGameObjectWithTag("Player");
       
        if (Time.time > (1 / fireRate) + lastFireTime)
        {
            //Debug.Log($"Time.time {Time.time}");
            Quaternion projectileRotation = Quaternion.LookRotation(
                 target.transform.position - enemy.transform.position);

            GameObject projectileInstance = Instantiate(
                projectilePrefab, enemy.transform.Find("ProjectileSpawnPoint").transform.position, projectileRotation);

            NetworkServer.Spawn(projectileInstance, player.connectionToClient);



            lastFireTime = Time.time;
        }

    }
}