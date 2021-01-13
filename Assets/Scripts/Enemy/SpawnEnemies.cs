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
    private GameObject defendObject;

    private Dictionary<int, List<BehaviorTree>> enemyBehaviorTreeGroup = new Dictionary<int, List<BehaviorTree>>();
   
    private enum BehaviorSelectionType { Attack, Charge, MarchingFire, Flank, Ambush, ShootAndScoot, Leapfrog, Surround, Defend, Hold, Retreat, Reinforcements, Last }
    private BehaviorSelectionType selectionType = BehaviorSelectionType.Attack;
    private BehaviorSelectionType prevSelectionType = BehaviorSelectionType.Attack;
    private int spawnBossCount = 1;
    private int spawnMoveRange = 1;
    private string ENEMY_TAG = "Player1";
    private string TARGET_TAG = "Player0";

    public override void OnStartServer()
    {
        mainCamera = Camera.main;
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        if (FindObjectOfType<NetworkManager>().numPlayers == 1){

            SpawnEnemyBase();
            StartCoroutine(loadBoss(2f));
            StartCoroutine(loadEnemy(2f));
            
            InvokeRepeating("addBehaviourToMilitary", 5f, 6000000f);
            //InvokeRepeating("TryDefend", 10f, 6000000f);
            InvokeRepeating("TryFlank", 5f, 6000000f);
        }
    }
   
    private void SpawnEnemyBase()
    {

        Vector3 pos = GameObject.FindGameObjectsWithTag("SpawnPoint")[1].transform.position;
        defendObject = Instantiate( unitBasePrefab,pos, Quaternion.identity);
        defendObject.tag = "EnemyBase";
        defendObject.SetActive(true);
        NetworkServer.Spawn(defendObject, player.connectionToClient);
    }
    private IEnumerator loadEnemy(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        while (spawncount > 0)
        {
            GameObject[] points = GameObject.FindGameObjectsWithTag("SpawnPoint");

            Vector3 spawnPosition = points[3].transform.position;
            Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
            spawnOffset.y = spawnPosition.y;
            enemy = Instantiate(enemyPrefab, spawnPosition + spawnOffset, Quaternion.identity) as GameObject;
            enemy.tag = ENEMY_TAG;
            enemy.GetComponent<Unit>().unitType = Unit.UnitType.ARCHER;

            //Debug.Log($"spawnEnemy connectionToClient {player.connectionToClient}");
            NetworkServer.Spawn(enemy, player.connectionToClient);
            spawncount--;
        }
    }

    private IEnumerator loadBoss(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (spawnBossCount > 0)
        {
            GameObject unit;
            GameObject[] points = GameObject.FindGameObjectsWithTag("SpawnPoint");
            Vector3 spawnPosition = points[3].transform.position;
            Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
            spawnOffset.y = spawnPosition.y;
            unit = Instantiate(bossPrefab, spawnPosition + spawnOffset, Quaternion.identity) as GameObject;
            unit.name = "Boss";
            unit.tag = ENEMY_TAG;
            unit.GetComponent<Unit>().unitType = Unit.UnitType.HERO;
            NetworkServer.Spawn(unit, player.connectionToClient);
            
        }
    }
   
  
    private void addBehaviourToMilitary()
    {

        GameObject boss = null;
        GameObject[] armies = GameObject.FindGameObjectsWithTag(ENEMY_TAG);
        foreach (GameObject child in armies)
        {
            if (child.gameObject.name.ToUpper().Contains("BOSS")) { boss = child; }
            child.transform.parent = enemyGroup.transform;
        }

        for (int i = 0; i < enemyGroup.transform.childCount; ++i)
        {
            var child = enemyGroup.transform.GetChild(i);
            var agentTrees = child.GetComponents<BehaviorTree>();
            for (int j = 0; j < agentTrees.Length; ++j)
            {
                var group = agentTrees[j].Group;

                agentTrees[j].SetVariableValue("newTargetName", TARGET_TAG);
                if (j == (int)BehaviorSelectionType.Hold || j == (int)BehaviorSelectionType.Defend)
                {
                    agentTrees[j].SetVariableValue("newDefendObject", defendObject);
                }
                if (!child.gameObject.name.ToUpper().Contains("BOSS"))
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
        GameObject[] armies = GameObject.FindGameObjectsWithTag(ENEMY_TAG);

        yield return new WaitForSeconds(0.1f);
        foreach (GameObject army in armies)
        {
            //Debug.Log($"SE -> EnableBehavior -> NetworkAnimator -> {army} -> wait");
            army.GetComponent<Unit>().GetUnitMovement().unitNetworkAnimator.SetTrigger("wait");
        }
        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < enemyBehaviorTreeGroup[(int)selectionType].Count; ++i)
        {
            if (enemyBehaviorTreeGroup[(int)selectionType][i] != null)
                enemyBehaviorTreeGroup[(int)selectionType][i].EnableBehavior();
            //Debug.Log($"(int)selectionType {(int)selectionType} / {i} ==== {agentBehaviorTreeGroup[(int)selectionType][i]}");
        }
        foreach (GameObject army in armies)
        {
            //Debug.Log($"SE -> EnableBehavior -> NetworkAnimator -> {army} -> run");
            army.GetComponent<Unit>().GetUnitMovement().unitNetworkAnimator.SetTrigger("run");
        }

    }


}