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
        RTSPlayer player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        enemyID = player.GetEnemyID();
        playerID = player.GetPlayerID();
        tacticalBehavior = GameObject.FindObjectOfType<TacticalBehavior>();
        SpawnEnemyBase();
        InvokeRepeating("LoadEnemies", 2f, 99999999999f);
        StartCoroutine(TryTactical(TacticalBehavior.BehaviorSelectionType.Defend));
    }

    public void LoadEnemies()
    {
   
        foreach (GameObject factroy in GameObject.FindGameObjectsWithTag("UnitFactory"))
        {
            if (factroy.GetComponent<UnitFactory>().hasAuthority)
            {
                localFactory = factroy.GetComponent<UnitFactory>();
                localFactory.CmdSpawnUnit(Unit.UnitType.MAGE,  1 , enemyID, unitAuthority);
                localFactory.CmdSpawnUnit(Unit.UnitType.MINISKELETON, 10, enemyID, unitAuthority);
            }
        }
    }

    private void SpawnEnemyBase()
    {
        Vector3 pos = GameObject.FindGameObjectsWithTag("SpawnPoint")[1].transform.position;
        GameObject defendObject = Instantiate(capsulePrefab, pos, Quaternion.identity);
        defendObject.tag = "PlayerBase" + enemyID;
        defendObject.SetActive(true);
    }

    private IEnumerator TryTactical(TacticalBehavior.BehaviorSelectionType type)
    {
        Debug.Log($"Spawn Enemy TryTactical --> TacticalFormation enemyID {enemyID}");
        StartCoroutine(tacticalBehavior.TacticalFormation(enemyID, playerID));
        yield return new WaitForSeconds(6f);
        tacticalBehavior.TryTB((int) type , enemyID);
    }


}