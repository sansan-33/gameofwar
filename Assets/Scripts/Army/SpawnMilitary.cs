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
    private int spawnMoveRange = 1;

    private int initArcherCount = 0;
    private int initFootmanCount = 0;
    private int initKnightCount = 0;
    private int initHeroCount = 0;

    [SerializeField] private float fireRate = 6000f;
    private RTSPlayer player;
    public override void OnStartServer()
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        if (FindObjectOfType<NetworkManager>().numPlayers == 1) {

            StartCoroutine(loadArcher(initArcherCount, 2f));
            StartCoroutine(loadHero(initFootmanCount, 2f));
            StartCoroutine(loadKnight(initKnightCount, 2f));
            StartCoroutine(loadFootman(initHeroCount, 2f));
            //InvokeRepeating("TrySlash", 10f, 2f);
            //InvokeRepeating("TryShoot", 3f, 10f);
        }

    }
    private void Update()
    {
       
    }
    public void SpawnUnit(Unit.UnitType unitType, int numberOfUnit)
    {
        switch (unitType)
        {
            case Unit.UnitType.ARCHER:
                StartCoroutine(loadArcher(numberOfUnit, 1f));
                break;
            case Unit.UnitType.HERO:
                StartCoroutine(loadHero(numberOfUnit, 1f));
                break;
            case Unit.UnitType.KNIGHT:
                StartCoroutine(loadKnight(numberOfUnit, 1f));
                break;
           case Unit.UnitType.SPEARMAN:
                StartCoroutine(loadFootman(numberOfUnit, 1f));
                break;

        }
    }


    private IEnumerator loadArcher(int spawnArcherCount , float waitTime)
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

    private IEnumerator loadFootman(int spawnFootmanCount , float waitTime)
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
    private IEnumerator loadKnight(int spawnKnightCount, float waitTime)
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
    private IEnumerator loadHero(int spawnHeroCount ,float waitTime)
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
    
   
}