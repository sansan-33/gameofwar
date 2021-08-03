using System.Collections;
using UnityEngine;
using Mirror;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.Networking;
using System.Text;
using System;
using System.Linq;

public class SpawnEnemies : MonoBehaviour
{
    private UnitFactory localFactory;

    private int enemyID = 0;
    private int playerID = 0;
    private Color teamColor;
    private bool ISGAMEOVER = false;
    [SerializeField] public Dictionary<string, CardStats> userCardStatsDict = new Dictionary<string, CardStats>();
    public TacticalBehavior tacticalBehavior;

    void Start()
    {
        if (NetworkClient.connection.identity == null) { return; }
        StartCoroutine(GetUserCard("-1", ""));

        //Debug.Log($"Spawn Enemies Awake {NetworkClient.connection.identity} NetworkManager number of players ? {((RTSNetworkManager)NetworkManager.singleton).Players.Count  } ");
        if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)
        {
            RTSPlayer player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
            enemyID = player.GetEnemyID();
            playerID = player.GetPlayerID();
            teamColor = player.GetTeamEnemyColor();
            //StartCoroutine(GetUserCard("-1",""));
            GameStartDisplay.ServerGameStart += LoadEnemies;
            //InvokeRepeating("LoadEnemies", 3f, 3f);
            GameOverHandler.ClientOnGameOver += HandleGameOver;
        }
    }
    public void OnDestroy()
    {
        GameStartDisplay.ServerGameStart -= LoadEnemies;
        GameOverHandler.ClientOnGameOver -= HandleGameOver;
    }
    public void LoadEnemies()
    {
        //if (ISGAMEOVER) { return; }
        foreach (GameObject factroy in GameObject.FindGameObjectsWithTag("UnitFactory"))
        {
            if (factroy.GetComponent<UnitFactory>().hasAuthority)
            {
                localFactory = factroy.GetComponent<UnitFactory>();
                StartCoroutine(HandleLoadEnemies());
            }
        }
    }
    IEnumerator HandleLoadEnemies()
    {
        CardStats cardStats;
        UnitMeta.Race race;
        if (StaticClass.Chapter == "1"&&StaticClass.Mission == "5")
        {
            race = UnitMeta.Race.ELF;
        }
        else
        {
            race = StaticClass.Chapter == null ? UnitMeta.Race.ELF : (UnitMeta.Race)Enum.Parse(typeof(UnitMeta.Race), (int.Parse(StaticClass.Chapter) - 1).ToString());
        }
     

        if (isUnitAlive(UnitMeta.UnitType.KING) < 1)
        {
            //Debug.Log($"LoadEnemies {UnitMeta.UnitRaceTypeKey[race][UnitMeta.UnitType.KING].ToString()}");
            cardStats = userCardStatsDict[UnitMeta.GetUnitKeyByRaceType(race,UnitMeta.UnitType.KING).ToString()];
            localFactory.CmdSpawnUnitRotation(race, UnitMeta.UnitType.KING, 1, enemyID, cardStats.cardLevel, cardStats.health, cardStats.attack, cardStats.repeatAttackDelay, cardStats.speed, cardStats.defense, cardStats.special, cardStats.specialkey, cardStats.passivekey, teamColor, Quaternion.Euler(0, 180, 0)); ;
        }

        if (isUnitAlive(UnitMeta.UnitType.HERO) < 1)
        {
            cardStats = userCardStatsDict[UnitMeta.GetUnitKeyByRaceType(race,UnitMeta.UnitType.HERO).ToString()];
            localFactory.CmdSpawnUnit(race, UnitMeta.UnitType.HERO, 1, enemyID, cardStats.cardLevel, cardStats.health, cardStats.attack, cardStats.repeatAttackDelay, cardStats.speed, cardStats.defense, cardStats.special, cardStats.specialkey, cardStats.passivekey, teamColor);
        }

        if (isUnitAlive(UnitMeta.UnitType.QUEEN) < 1)
        {
            cardStats = userCardStatsDict[UnitMeta.GetUnitKeyByRaceType(race,UnitMeta.UnitType.QUEEN).ToString()];
            localFactory.CmdSpawnUnit(race, UnitMeta.UnitType.QUEEN, 1, enemyID, cardStats.cardLevel, cardStats.health, cardStats.attack, cardStats.repeatAttackDelay, cardStats.speed, cardStats.defense, cardStats.special, cardStats.specialkey, cardStats.passivekey, teamColor);
        }

        while (!ISGAMEOVER)
        {
            if (isUnitAlive(UnitMeta.UnitType.MAGIC) < 1)
            {
                if (StaticClass.Mission != "3" || StaticClass.Chapter != "1")
                {
                    cardStats = userCardStatsDict[UnitMeta.GetUnitKeyByRaceType(race, UnitMeta.UnitType.MAGIC).ToString()];
                    localFactory.CmdSpawnUnit(race, UnitMeta.UnitType.MAGIC, 1, enemyID, cardStats.cardLevel, cardStats.health, cardStats.attack, cardStats.repeatAttackDelay, cardStats.speed, cardStats.defense, cardStats.special, cardStats.specialkey, cardStats.passivekey, teamColor);

                }
            }
            if (isUnitAlive(UnitMeta.UnitType.TANK) < 0)
            {
                cardStats = userCardStatsDict[UnitMeta.GetUnitKeyByRaceType(race,UnitMeta.UnitType.TANK).ToString()];
                localFactory.CmdSpawnUnit(race, UnitMeta.UnitType.TANK, 1, enemyID, cardStats.cardLevel, cardStats.health, cardStats.attack, cardStats.repeatAttackDelay, cardStats.speed, cardStats.defense, cardStats.special, cardStats.specialkey, cardStats.passivekey, teamColor);
            }

            if (isUnitAlive(UnitMeta.UnitType.ARCHER) < 0)
            {
                cardStats = userCardStatsDict[UnitMeta.GetUnitKeyByRaceType(race,UnitMeta.UnitType.ARCHER).ToString()];
                localFactory.CmdSpawnUnit(race, UnitMeta.UnitType.ARCHER, 1, enemyID, cardStats.cardLevel, cardStats.health, cardStats.attack, cardStats.repeatAttackDelay, cardStats.speed, cardStats.defense, cardStats.special, cardStats.specialkey, cardStats.passivekey, teamColor);
            }

            if (isUnitAlive(UnitMeta.UnitType.FOOTMAN) < 0)
            {
                cardStats = userCardStatsDict[UnitMeta.GetUnitKeyByRaceType(race,UnitMeta.UnitType.FOOTMAN).ToString()];
                localFactory.CmdSpawnUnit(race, UnitMeta.UnitType.FOOTMAN, 1, enemyID, cardStats.cardLevel, cardStats.health, cardStats.attack, cardStats.repeatAttackDelay, cardStats.speed, cardStats.defense, cardStats.special, cardStats.specialkey, cardStats.passivekey, teamColor);
            }

            StartCoroutine(TryTactical(UnitMeta.UnitType.ARCHER, TacticalBehavior.BehaviorSelectionType.Attack));
            StartCoroutine(TryTactical(UnitMeta.UnitType.FOOTMAN, TacticalBehavior.BehaviorSelectionType.Attack));
            StartCoroutine(TryTactical(UnitMeta.UnitType.HERO, TacticalBehavior.BehaviorSelectionType.Defend));
            StartCoroutine(TryTactical(UnitMeta.UnitType.TANK, TacticalBehavior.BehaviorSelectionType.Attack));
            yield return new WaitForSeconds(3f);

        }
        yield return null;
    }
    
    private IEnumerator TryTactical(UnitMeta.UnitType unitType , TacticalBehavior.BehaviorSelectionType selectionType)
    {
        //Debug.Log($"Spawn Enemy TryTactical --> TacticalFormation enemyID {enemyID}");
        if (GameObject.FindGameObjectsWithTag(UnitMeta.KINGPLAYERTAG).Length == 0) { yield break; }
        tacticalBehavior.SetKingBoss(enemyID, GameObject.FindGameObjectWithTag(UnitMeta.KINGENEMYTAG));
        yield return tacticalBehavior.TacticalFormation(enemyID, playerID,null);
        //tacticalBehavior.TryTB((int)selectionType, enemyID, (int) unitType);
    }
    private int isUnitAlive(UnitMeta.UnitType unitType)
    {
        int isAlive = 0;
        List<GameObject> armies = new List<GameObject>();
        GameObject[] units = GameObject.FindGameObjectsWithTag(unitType == UnitMeta.UnitType.KING ? UnitMeta.KINGENEMYTAG : UnitMeta.ENEMYTAG);
        armies = units.ToList(); 
        GameObject[] tank = GameObject.FindGameObjectsWithTag("Provoke1");
        if (tank != null)
        {
            armies.AddRange(tank.ToList());
        }
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
            //Debug.Log($"{i} : {jsonResult[i]["cardkey"]}");
            if (jsonResult[i]["cardkey"] != null && jsonResult[i]["cardkey"].ToString().Length > 0)
            {
                userCardStatsDict.Add(jsonResult[i]["cardkey"], new CardStats(jsonResult[i]["star"], jsonResult[i]["level"], jsonResult[i]["health"], jsonResult[i]["attack"], jsonResult[i]["repeatattackdelay"], jsonResult[i]["speed"], jsonResult[i]["defense"], jsonResult[i]["special"], jsonResult[i]["specialkey"], jsonResult[i]["passivekey"]));
            }
        }

        //Debug.Log($"Spawn enemies GetUserCard {webReq.url } jsonResult: {jsonResult}");
    }
}