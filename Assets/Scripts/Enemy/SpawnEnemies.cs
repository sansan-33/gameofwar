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
    private Color teamColor;
    private bool ISGAMEOVER = false;

    public TacticalBehavior tacticalBehavior;
    void Start()
    {
        if (NetworkClient.connection.identity == null) { return; }
        //Debug.Log($"Spawn Enemies Awake {NetworkClient.connection.identity} NetworkManager number of players ? {((RTSNetworkManager)NetworkManager.singleton).Players.Count  } ");
        if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)
        {
            RTSPlayer player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
            enemyID = player.GetEnemyID();
            playerID = player.GetPlayerID();
            teamColor = player.GetTeamColor();
            teamColor = player.GetTeamEnemyColor();
            InvokeRepeating("LoadEnemies", 2f, 3f);
        }
        GameOverHandler.ClientOnGameOver += HandleGameOver;
    }

    public void LoadEnemies()
    {
        if (ISGAMEOVER) { return; }
        foreach (GameObject factroy in GameObject.FindGameObjectsWithTag("UnitFactory"))
        {
            if (factroy.GetComponent<UnitFactory>().hasAuthority)
            {
                localFactory = factroy.GetComponent<UnitFactory>();
                if (isUnitAlive(UnitMeta.UnitType.KING) < 1)
                {
                    localFactory.CmdSpawnUnitRotation(UnitMeta.Race.UNDEAD,  UnitMeta.UnitType.KING, 1, enemyID, unitAuthority, teamColor, Quaternion.Euler(0, 180, 0));
                }
                
                if (isUnitAlive(UnitMeta.UnitType.HERO ) < 1)
                {
                    localFactory.CmdSpawnUnit(UnitMeta.Race.UNDEAD, UnitMeta.UnitType.HERO, 1, enemyID, unitAuthority, teamColor);
                }
                
                if (isUnitAlive(UnitMeta.UnitType.TANK) < 1) { 
                    localFactory.CmdSpawnUnit(UnitMeta.Race.UNDEAD, UnitMeta.UnitType.TANK, 1, enemyID, unitAuthority, teamColor);
                }
                if (isUnitAlive(UnitMeta.UnitType.ARCHER) < 1)
                {
                    localFactory.CmdSpawnUnit(UnitMeta.Race.UNDEAD, UnitMeta.UnitType.ARCHER, 1, enemyID, unitAuthority, teamColor);
                }
                if (isUnitAlive(UnitMeta.UnitType.FOOTMAN) < 12)
                { 
                    localFactory.CmdSpawnUnit(UnitMeta.Race.UNDEAD, UnitMeta.UnitType.FOOTMAN, 1, enemyID, unitAuthority, teamColor);
                }
                
                StartCoroutine(TryTactical(UnitMeta.UnitType.ARCHER, TacticalBehavior.BehaviorSelectionType.Attack));
                StartCoroutine(TryTactical(UnitMeta.UnitType.FOOTMAN, TacticalBehavior.BehaviorSelectionType.Attack));
                StartCoroutine(TryTactical(UnitMeta.UnitType.HERO, TacticalBehavior.BehaviorSelectionType.Defend));
                StartCoroutine(TryTactical(UnitMeta.UnitType.TANK, TacticalBehavior.BehaviorSelectionType.Attack));
            }
        }
    }
    
    private IEnumerator TryTactical(UnitMeta.UnitType unitType , TacticalBehavior.BehaviorSelectionType selectionType)
    {
        //Debug.Log($"Spawn Enemy TryTactical --> TacticalFormation enemyID {enemyID}");
        if (GameObject.FindGameObjectsWithTag(UnitMeta.KINGPLAYERTAG).Length == 0) { yield break; }
        tacticalBehavior.SetKingBoss(enemyID, GameObject.FindGameObjectWithTag(UnitMeta.KINGENEMYTAG));
        yield return tacticalBehavior.TacticalFormation(enemyID, playerID);
        tacticalBehavior.TryTB((int)selectionType, enemyID, (int) unitType);
    }
    private int isUnitAlive(UnitMeta.UnitType unitType)
    {
        int isAlive = 0;
        GameObject[] armies = GameObject.FindGameObjectsWithTag(unitType == UnitMeta.UnitType.KING ? UnitMeta.KINGENEMYTAG : UnitMeta.ENEMYTAG);
        foreach (GameObject child in armies)
        {
            if (child.GetComponent<Unit>().unitType == unitType) { 
                isAlive++;
            }
        }
        return isAlive;
    }
    public void HandleGameOver(string winner)
    {
        //Debug.Log($"Spawn Enemies ==> HandleGameOver");
        ISGAMEOVER = true;
    }
}