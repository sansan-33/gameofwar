using System.Collections;
using UnityEngine;
using Mirror;

public class SpawnEnemies : MonoBehaviour
{
    [SerializeField] private GameObject capsulePrefab;
    private UnitFactory localFactory;
  
    private bool unitAuthority = false;
    private int enemyID = 0;
    private int playerID = 0;
    private TacticalBehavior tacticalBehavior;
    void Awake()
    {
        if (NetworkClient.connection.identity == null) { return; }
        Debug.Log($"Spawn Enemies Awake {NetworkClient.connection.identity} NetworkManager number of players ? {((RTSNetworkManager)NetworkManager.singleton).Players.Count  } ");
        if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)
        {
            RTSPlayer player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
            enemyID = player.GetEnemyID();
            playerID = player.GetPlayerID();
            Debug.Log($"Number of player : {((RTSNetworkManager)NetworkManager.singleton).Players.Count} enemyID {enemyID} playerID {playerID} ");

            tacticalBehavior = GameObject.FindObjectOfType<TacticalBehavior>();
            SpawnEnemyBase();
            InvokeRepeating("LoadEnemies", 2f, 20f);
            
        }
    }

    public void LoadEnemies()
    {
   
        foreach (GameObject factroy in GameObject.FindGameObjectsWithTag("UnitFactory"))
        {
            if (factroy.GetComponent<UnitFactory>().hasAuthority)
            {
                localFactory = factroy.GetComponent<UnitFactory>();
                localFactory.CmdSpawnUnit(Unit.UnitType.SPEARMAN,  1 , enemyID, unitAuthority);
                //localFactory.CmdSpawnUnit(Unit.UnitType.MINISKELETON, 10, enemyID, unitAuthority);
                StartCoroutine(TryTactical(TacticalBehavior.BehaviorSelectionType.Defend));
            }
        }
    }

    private void SpawnEnemyBase()
    {
        Vector3 pos = GameObject.FindGameObjectWithTag("SpawnPointEnemy").transform.position;
        GameObject defendObject = Instantiate(capsulePrefab, pos, Quaternion.identity);
        defendObject.tag = "PlayerBase" + enemyID;
        defendObject.SetActive(true);
    }

    private IEnumerator TryTactical(TacticalBehavior.BehaviorSelectionType type)
    {
        Debug.Log($"Spawn Enemy TryTactical --> TacticalFormation enemyID {enemyID}");
        StartCoroutine(tacticalBehavior.TacticalFormation(enemyID, playerID));
        yield return new WaitForSeconds(5f);
        tacticalBehavior.TryTB((int) type , enemyID);
    }


}