using System.Collections;
using UnityEngine;
using Mirror;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.Networking;
using System.Text;
using System;
using System.Linq;

public class TeamSpawner : MonoBehaviour
{
    private UnitFactory localFactory;

    private int playerID = 0;
    private Color teamColor;
    private Dictionary<string, JSONNode> userTeamDict = new Dictionary<string, JSONNode>();

    void Start()
    {
        if (NetworkClient.connection.identity == null) { return; }
        //Debug.Log($"Spawn Enemies Awake {NetworkClient.connection.identity} NetworkManager number of players ? {((RTSNetworkManager)NetworkManager.singleton).Players.Count  } ");
        RTSPlayer player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        playerID = player.GetPlayerID();
        teamColor = player.GetTeamColor();
        StartCoroutine(loadMilitary(2f,player, Quaternion.identity)); //Quaternion.Euler(0, 180, 0)

    }
    private IEnumerator loadMilitary(float waitTime, RTSPlayer player, Quaternion rotation)
    {
        yield return LoadUserTeam(player.GetUserID(), player.GetPlayerID());
        yield return new WaitForSeconds(waitTime);

        CardStats cardStats;
        JSONNode userTeamCard;
        string userkey = player.GetUserID() + "_" + player.GetPlayerID();
        Dictionary<UnitMeta.UnitKey, Unit> playerUnitDict = new Dictionary<UnitMeta.UnitKey, Unit>();
        Unit unit;
        GameObject[] units = GameObject.FindGameObjectsWithTag("Player" + player.GetPlayerID());
        GameObject king = GameObject.FindGameObjectWithTag("King" + player.GetPlayerID());
        List<GameObject> armies = new List<GameObject>();
        armies = units.ToList();
        if (king != null)
            armies.Add(king);
        foreach (GameObject child in armies)
        {
            playerUnitDict.Add(child.GetComponent<Unit>().unitKey, child.GetComponent<Unit>());
        }

        foreach (GameObject factroy in GameObject.FindGameObjectsWithTag("UnitFactory"))
        {
            if (factroy.GetComponent<UnitFactory>().hasAuthority)
            {
                localFactory = factroy.GetComponent<UnitFactory>();
                for (int i = 0; i < userTeamDict[userkey].Count; i++)
                {
                    userTeamCard = userTeamDict[userkey][i];
                    UnitMeta.UnitKey unitKey = (UnitMeta.UnitKey)Enum.Parse(typeof(UnitMeta.UnitKey), userTeamCard["cardkey"]);
                    cardStats = new CardStats( 1, userTeamCard["level"], userTeamCard["health"], userTeamCard["attack"], userTeamCard["repeatattackdelay"], userTeamCard["speed"], userTeamCard["defense"], userTeamCard["special"], userTeamCard["specialkey"], userTeamCard["passivekey"]);
                    // Debug.Log($"loadMilitary {unitKey} {UnitMeta.KeyType[unitKey]}");
                    // localFactory.CmdSpawnTeamUnit(unitKey, cardStats.star, playerID, cardStats.cardLevel, cardStats.health, cardStats.attack, cardStats.repeatAttackDelay, cardStats.speed, cardStats.defense, cardStats.special, cardStats.specialkey, cardStats.passivekey, teamColor, rotation); 
                    unit = playerUnitDict[unitKey];
                    //unit.GetComponent<UnitPowerUp>().CmdPowerUp(unit.gameObject, cardStats.star, cardStats.cardLevel, cardStats.health, cardStats.attack, cardStats.repeatAttackDelay, cardStats.speed, cardStats.defense, cardStats.special, cardStats.specialkey, cardStats.passivekey);
                    //unit.GetComponent<UnitPowerUp>().CmdTag(unit.gameObject, playerID, unitKey.ToString(), teamColor, unit.GetSpawnPointIndex(), cardStats.star, cardStats.cardLevel, cardStats.health, cardStats.attack, cardStats.repeatAttackDelay, cardStats.speed, cardStats.defense, cardStats.special, cardStats.specialkey, cardStats.passivekey);
                    //unit.GetComponent<UnitPowerUp>().ServerPowerUp(unit.gameObject, cardStats.star, cardStats.cardLevel, cardStats.health, cardStats.attack, cardStats.repeatAttackDelay, cardStats.speed, cardStats.defense, cardStats.special, cardStats.specialkey, cardStats.passivekey);

                }
            }
        }
    }

    public IEnumerator LoadUserTeam(string userid, int playerid)
    {
        userTeamDict.Clear();
        JSONNode jsonResult;
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();
        webReq.url = string.Format("{0}/{1}/{2}", APIConfig.urladdress, APIConfig.teamCardService, userid);
        yield return webReq.SendWebRequest();
        string rawJson = Encoding.Default.GetString(webReq.downloadHandler.data);
        jsonResult = JSON.Parse(rawJson);
        userTeamDict.Add(userid + "_" + playerid, jsonResult);
        Debug.Log($"LoadUserTeam jsonResult {webReq.url } {jsonResult}");

    }

}