using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;

public class SpawnEnemies : NetworkBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private GameObject unitBasePrefab;
    [SerializeField] private GameObject projectilePrefab = null;
    [SerializeField] private float fireRange = 30;

    private NavMeshAgent agent = null;
    private Camera mainCamera;

    [SerializeField]
    private float spawnInterval = 0.1f;

    private GameObject enemy;
    private float stoppingDistance = 1;
    private float chaseRange = 1;
    private int spawncount=5;
    private float lastFireTime;
    [SerializeField] private float fireRate = 6000f;
    private RTSPlayer player;


    public GameObject enemyGroup;
    public GameObject defendObject;

    private Dictionary<int, List<BehaviorTree>> enemyBehaviorTreeGroup = new Dictionary<int, List<BehaviorTree>>();
    private Health[] enemyHealth;

    private enum BehaviorSelectionType { Attack, Charge, MarchingFire, Flank, Ambush, ShootAndScoot, Leapfrog, Surround, Defend, Hold, Retreat, Reinforcements, Last }
    private BehaviorSelectionType selectionType = BehaviorSelectionType.Attack;
    private BehaviorSelectionType prevSelectionType = BehaviorSelectionType.Attack;
    private int spawnBossCount = 1;
    private int spawnMoveRange = 1;

    public override void OnStartServer()
    {
        mainCamera = Camera.main;
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        if (FindObjectOfType<NetworkManager>().numPlayers == 1){

            //SpawnEnemyBase();
            StartCoroutine(loadBoss(2f));

            while (spawncount > 0)
            {
                InvokeRepeating("SpawnEnemy", 0.1f, this.spawnInterval);
                spawncount--;
            }

            InvokeRepeating("addBehaviourToMilitary", 5f, 6000000f);
            InvokeRepeating("TryDefend", 10f, 6000000f);
            //InvokeRepeating("TrySurround", 30f, 6000000f);
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
    private void SpawnEnemyBase()
    {

        Vector3 pos = GameObject.FindGameObjectsWithTag("SpawnPoint")[2].transform.position;
        GameObject baseInstance = Instantiate( unitBasePrefab,pos, Quaternion.identity);
        baseInstance.tag = "EnemyBase";
        NetworkServer.Spawn(baseInstance, player.connectionToClient);
    }
    private void SpawnEnemy()
    {

            int i = 0;
            int spawnMoveRange = 1;
            GameObject[] points = GameObject.FindGameObjectsWithTag("SpawnPoint");
            
            Vector3 spawnPosition = points[2].transform.position ;
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
            //InvokeRepeating("TryShoot", 1f, 1f);
    }

    private IEnumerator loadBoss(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (spawnBossCount > 0)
        {
            GameObject unit;
            NavMeshAgent agent = null;

            GameObject[] points = GameObject.FindGameObjectsWithTag("SpawnPoint");
            Vector3 spawnPosition = points[2].transform.position;
            Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
            spawnOffset.y = spawnPosition.y;
            unit = Instantiate(bossPrefab, spawnPosition + spawnOffset, Quaternion.identity) as GameObject;
            unit.name = "Boss";
            NetworkServer.Spawn(unit, player.connectionToClient);
            unit.GetComponent<Targeter>().CmdSetAttackType(Targeter.AttackType.Slash);

            unit.GetComponent<Unit>().unitType = Unit.UnitType.HERO;
            agent = unit.GetComponent<NavMeshAgent>();
            agent.SetDestination(spawnPosition + spawnOffset);
        }
    }
    private void TryMove()
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

  
    private void addBehaviourToMilitary()
    {

        GameObject boss = null;
        GameObject[] armies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject child in armies)
        {
            if (child.gameObject.name.Contains("Boss")) { boss = child; }
            child.transform.parent = enemyGroup.transform;
        }

        for (int i = 0; i < enemyGroup.transform.childCount; ++i)
        {
            var child = enemyGroup.transform.GetChild(i);
            var agentTrees = child.GetComponents<BehaviorTree>();
            for (int j = 0; j < agentTrees.Length; ++j)
            {
                var group = agentTrees[j].Group;

                agentTrees[j].SetVariableValue("newTargetName", "Player");
                if (j == (int)BehaviorSelectionType.Hold || j == (int)BehaviorSelectionType.Defend)
                {
                    agentTrees[j].SetVariableValue("newDefendObject", defendObject);
                }
                if (!child.gameObject.name.Contains("Boss"))
                {
                    agentTrees[j].SetVariableValue("newLeader", boss);
                }
                else
                {
                    agentTrees[j].SetVariableValue("newLeader", null);
                }

                List<BehaviorTree> groupBehaviorTrees;
                if (!enemyBehaviorTreeGroup.TryGetValue(group, out groupBehaviorTrees))
                {
                    groupBehaviorTrees = new List<BehaviorTree>();
                    enemyBehaviorTreeGroup.Add(group, groupBehaviorTrees);
                }
                groupBehaviorTrees.Add(agentTrees[j]);
            }
        }

    }

    public void TryAttack()
    {
        prevSelectionType = selectionType;
        selectionType = BehaviorSelectionType.Attack;
        SelectionChanged();
    }
    public void TryDefend()
    {
        prevSelectionType = selectionType;
        selectionType = BehaviorSelectionType.Defend;
        SelectionChanged();
    }
    public void TryAmbush()
    {
        prevSelectionType = selectionType;
        selectionType = BehaviorSelectionType.Ambush;
        SelectionChanged();
    }
    public void TryRetreat()
    {
        prevSelectionType = selectionType;
        selectionType = BehaviorSelectionType.Retreat;
        SelectionChanged();

    }
    public void TryFlank()
    {
        prevSelectionType = selectionType;
        selectionType = BehaviorSelectionType.Flank;
        SelectionChanged();

    }
    public void TrySurround()
    {
        prevSelectionType = selectionType;
        selectionType = BehaviorSelectionType.Surround;
        SelectionChanged();

    }

    private void SelectionChanged()
    {
        StopCoroutine("EnableBehavior");
        for (int i = 0; i < enemyBehaviorTreeGroup[(int)prevSelectionType].Count; ++i)
        {
            enemyBehaviorTreeGroup[(int)prevSelectionType][i].DisableBehavior();
        }

        StartCoroutine("EnableBehavior");
    }

    private IEnumerator EnableBehavior()
    {
        //defendObject.SetActive(false);

        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < enemyBehaviorTreeGroup[(int)selectionType].Count; ++i)
        {
            enemyBehaviorTreeGroup[(int)selectionType][i].EnableBehavior();
            //Debug.Log($"(int)selectionType {(int)selectionType} / {i} ==== {agentBehaviorTreeGroup[(int)selectionType][i]}");
        }
    }


}