using System.Collections;
using UnityEngine;
using Mirror;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.Networking;
using System.Text;

public class SpawnEnemies : MonoBehaviour
{
    [SerializeField] private GameObject capsulePrefab;
    private UnitFactory localFactory;

    private bool unitAuthority = false;
    private int enemyID = 0;
    private int playerID = 0;
    private Color teamColor;
    private bool ISGAMEOVER = false;
    [SerializeField] public Dictionary<string, Card_Stats> userCardStatsDict = new Dictionary<string, Card_Stats>();

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
            StartCoroutine(GetUserCard("-1",""));
            InvokeRepeating("LoadEnemies", 2f, 3f);
        }
        GameOverHandler.ClientOnGameOver += HandleGameOver;
    }

    public void LoadEnemies()
    {
        if (ISGAMEOVER) { return; }
        Card_Stats card_Stats;
        foreach (GameObject factroy in GameObject.FindGameObjectsWithTag("UnitFactory"))
        {
            if (factroy.GetComponent<UnitFactory>().hasAuthority)
            {
                localFactory = factroy.GetComponent<UnitFactory>();
                if (isUnitAlive(UnitMeta.UnitType.KING) < 1)
                {
                    card_Stats = userCardStatsDict[UnitMeta.UnitRaceTypeKey[UnitMeta.Race.UNDEAD][UnitMeta.UnitType.KING].ToString()];
                    localFactory.CmdSpawnUnitRotation(UnitMeta.Race.UNDEAD,  UnitMeta.UnitType.KING, 1, enemyID, card_Stats.health, card_Stats.attack, card_Stats.repeatAttackDelay, card_Stats.speed, card_Stats.defense, card_Stats.speed, teamColor, Quaternion.Euler(0, 180, 0));
                }
                
                if (isUnitAlive(UnitMeta.UnitType.HERO ) < 1)
                {
                    card_Stats = userCardStatsDict[UnitMeta.UnitRaceTypeKey[UnitMeta.Race.UNDEAD][UnitMeta.UnitType.KING].ToString()];
                    localFactory.CmdSpawnUnit(UnitMeta.Race.UNDEAD, UnitMeta.UnitType.HERO, 1, enemyID, card_Stats.health, card_Stats.attack, card_Stats.repeatAttackDelay, card_Stats.speed, card_Stats.defense, card_Stats.speed, teamColor);
                }
                
                if (isUnitAlive(UnitMeta.UnitType.TANK) < 1) {
                    card_Stats = userCardStatsDict[UnitMeta.UnitRaceTypeKey[UnitMeta.Race.UNDEAD][UnitMeta.UnitType.KING].ToString()];
                    localFactory.CmdSpawnUnit(UnitMeta.Race.UNDEAD, UnitMeta.UnitType.TANK, 1, enemyID, card_Stats.health, card_Stats.attack, card_Stats.repeatAttackDelay, card_Stats.speed, card_Stats.defense, card_Stats.speed, teamColor);
                }
                if (isUnitAlive(UnitMeta.UnitType.ARCHER) < 1)
                {
                    card_Stats = userCardStatsDict[UnitMeta.UnitRaceTypeKey[UnitMeta.Race.UNDEAD][UnitMeta.UnitType.KING].ToString()];
                    localFactory.CmdSpawnUnit(UnitMeta.Race.UNDEAD, UnitMeta.UnitType.ARCHER, 1, enemyID, card_Stats.health, card_Stats.attack, card_Stats.repeatAttackDelay, card_Stats.speed, card_Stats.defense, card_Stats.speed, teamColor);
                }
                if (isUnitAlive(UnitMeta.UnitType.FOOTMAN) < 12)
                {
                    card_Stats = userCardStatsDict[UnitMeta.UnitRaceTypeKey[UnitMeta.Race.UNDEAD][UnitMeta.UnitType.KING].ToString()];
                    localFactory.CmdSpawnUnit(UnitMeta.Race.UNDEAD, UnitMeta.UnitType.FOOTMAN, 1, enemyID, card_Stats.health, card_Stats.attack, card_Stats.repeatAttackDelay, card_Stats.speed, card_Stats.defense, card_Stats.speed, teamColor);
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
    // sends an API request - returns a JSON file
    IEnumerator GetUserCard(string userid, string race)
    {
        userCardStatsDict.Clear();
        JSONNode jsonResult;
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();
        webReq.url = string.Format("{0}/{1}/{2}", APIConfig.urladdress, APIConfig.cardService, userid, race);
        yield return webReq.SendWebRequest();

        string rawJson = Encoding.Default.GetString(webReq.downloadHandler.data);
        jsonResult = JSON.Parse(rawJson);
        for (int i = 0; i < jsonResult.Count; i++)
        {
            if (jsonResult[i]["cardkey"] != null && jsonResult[i]["cardkey"].ToString().Length > 0)
                userCardStatsDict.Add(jsonResult[i]["cardkey"], new Card_Stats(jsonResult[i]["level"], jsonResult[i]["health"], jsonResult[i]["attack"], jsonResult[i]["repeatAttackDelay"], jsonResult[i]["speed"], jsonResult[i]["defense"], jsonResult[i]["special"]));
        }
        Debug.Log($"jsonResult {webReq.url } {jsonResult}");
    }
}