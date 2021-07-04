using System.Collections;
using UnityEngine;
using Mirror;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.Networking;
using System.Text;
using System;
using System.Linq;

public class SpawnTeam : MonoBehaviour
{
    private UnitFactory localFactory;

    private int enemyID = 0;
    private int playerID = 0;
    private Color teamColor;
    private bool ISGAMEOVER = false;
    [SerializeField] public Dictionary<string, CardStats> userCardStatsDict = new Dictionary<string, CardStats>();
    private Dictionary<string, JSONNode> userTeamDict = new Dictionary<string, JSONNode>();
    RTSPlayer player ;

    public TacticalBehavior tacticalBehavior;

    void Start()
    {
        if (NetworkClient.connection.identity == null) { return; }
        
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        StartCoroutine(LoadUserTeam(player.GetUserID()));

        enemyID = player.GetEnemyID();
        playerID = player.GetPlayerID();
        teamColor = player.GetTeamEnemyColor();
        GameStartDisplay.ServerGameStart += LoadTeam;
    }
    public void OnDestroy()
    {
        GameStartDisplay.ServerGameStart -= LoadTeam;
    }
    public void LoadTeam()
    {
        foreach (GameObject factroy in GameObject.FindGameObjectsWithTag("UnitFactory"))
        {
            if (factroy.GetComponent<UnitFactory>().hasAuthority)
            {
                localFactory = factroy.GetComponent<UnitFactory>();
                StartCoroutine(HandleLoadTeam());
            }
        }
    }
    IEnumerator HandleLoadTeam()
    {
        CardStats cardStats;
        UnitMeta.UnitKey unitKey;
        UnitMeta.Race race = (UnitMeta.Race)Enum.Parse(typeof(UnitMeta.Race), player.GetRace()); 
        string userkey = player.GetUserID();
        JSONNode userTeamCard;
        for (int i = 0; i < userTeamDict[userkey].Count; i++) {
            userTeamCard = userTeamDict[userkey][i];
            unitKey = (UnitMeta.UnitKey)Enum.Parse(typeof(UnitMeta.UnitKey), userTeamCard["cardkey"]);  
            cardStats =  new CardStats(userTeamCard["star"], userTeamCard["level"], userTeamCard["health"], userTeamCard["attack"], userTeamCard["repeatattackdelay"], userTeamCard["speed"], userTeamCard["defense"], userTeamCard["special"], userTeamCard["specialkey"], userTeamCard["passivekey"]);
            localFactory.CmdSpawnTeamUnit( unitKey, 1, playerID, cardStats.cardLevel, cardStats.health, cardStats.attack, cardStats.repeatAttackDelay, cardStats.speed, cardStats.defense, cardStats.special, cardStats.specialkey, cardStats.passivekey, teamColor, Quaternion.Euler(0, 180, 0)); ;
        }
        yield return null;

    }

    IEnumerator LoadUserTeam(string userid)
    {
        userTeamDict.Clear();
        JSONNode jsonResult;
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();
        webReq.url = string.Format("{0}/{1}/{2}", APIConfig.urladdress, APIConfig.teamCardService, userid);
        yield return webReq.SendWebRequest();
        string rawJson = Encoding.Default.GetString(webReq.downloadHandler.data);
        jsonResult = JSON.Parse(rawJson);
        userTeamDict.Add(userid, jsonResult);
        //Debug.Log($"jsonResult {webReq.url } {jsonResult}");
    }
}