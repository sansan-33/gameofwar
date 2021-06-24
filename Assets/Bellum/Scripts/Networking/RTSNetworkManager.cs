using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mirror;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class RTSNetworkManager : NetworkManager
{
    [SerializeField] private GameObject undeadArcherPrefab = null;
    [SerializeField] private GameObject miniSkeletonPrefab = null;
    [SerializeField] private GameObject knightPrefab = null;
    [SerializeField] private GameObject heroPrefab = null;
    [SerializeField] private GameObject spearmanPrefab = null;
    [SerializeField] private GameObject unitFactoryPrefab = null;
    [SerializeField] private GameObject giantPrefab = null;
    [SerializeField] private GameObject magePrefab = null;
    [SerializeField] private GameObject cavalryPrefab = null;
    [SerializeField] private GameObject riderPrefab = null;
    [SerializeField] private GameObject kingPrefab = null;
    [SerializeField] private GameObject undeadHeroPrefab = null;
    [SerializeField] private GameObject undeadKingPrefab = null;
    [SerializeField] private GameObject undeadQueenPrefab = null;
    [SerializeField] private GameObject archerPrefab = null;
    [SerializeField] private GameObject odinPrefab = null;
    [SerializeField] private GameObject thorPrefab = null;
    [SerializeField] private GameObject lokiPrefab = null;
    [SerializeField] private GameObject elfTreeantPrefab = null;
    [SerializeField] private GameObject elfDemonHunterPrefab = null;
    [SerializeField] private GameObject elfQueenPrefab = null;

    [SerializeField] private GameOverHandler gameOverHandlerPrefab = null;
    [SerializeField] private GameBoardHandler gameBoardHandlerPrefab = null;
    [SerializeField] private GreatWallController greatWallPrefab = null;
    [SerializeField] private GameObject doorPrefab = null;
    private Dictionary<string, JSONNode> userTeamDict = new Dictionary<string, JSONNode>();

    public static event Action ClientOnConnected;
    public static event Action ClientOnDisconnected;
   
    private bool isGameInProgress = false;
    private bool isSinglePlayer = true;

    private List<Color> teamsColor = new List<Color>() { Color.blue, Color.red};
    public List<RTSPlayer> Players { get; } = new List<RTSPlayer>();

    private Dictionary<UnitMeta.UnitKey, GameObject> unitDict = new Dictionary<UnitMeta.UnitKey, GameObject>();
   
    #region Server
    public override void OnServerConnect(NetworkConnection conn)
    {
        if (!isGameInProgress) { return; }

        conn.Disconnect();
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();
        Players.Remove(player);
        isGameInProgress = false;

        if (Players.Count == 0){
            StartCoroutine(HandleEndGame(Convert.ToString((int)NetworkManager.singleton.GetComponent<TelepathyTransport>().port)));
        }
    }
    public override void OnStopServer()
    {
        Players.Clear();
        isGameInProgress = false;
    }
    public IEnumerator HandleEndGame(string port)
    {
        UnityWebRequest webReq = new UnityWebRequest();
        // build the url and query
        webReq.url = string.Format("{0}/{1}/{2}", APIConfig.urladdress, APIConfig.quitService , port);
        webReq.method = "put";
        webReq.SendWebRequest();
        yield return new WaitForSeconds(10f);
        //Application.Quit();
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
        //Debug.Log($"jsonResult {webReq.url } {jsonResult}");
    }
    public IEnumerator LoadTotalPower(string userid)
    {
        int TotalPower = 0;
        APIManager apiManager = new APIManager();
        yield return apiManager.GetTotalPower(userid, ChapterMissionMeta.ChapterMissionTeam[StaticClass.Chapter + "-" + StaticClass.Mission]);
        for (int i = 0; i < apiManager.data.Count; i++)
        {
            TotalPower += Int32.Parse(apiManager.data["GetTotalPower"][i]["power"]);
        }
        yield return null;
        //return TotalPower.ToString();
    }
    public void StartGame()
    {
        if (Players.Count > 1) { isSinglePlayer = false; }
       
        isGameInProgress = true;
        SetupUnitDict();
        ServerChangeScene("Scene_Map_0" + Random.Range(1, 4));
    }

    public void StartMission(string chapter, string mission)
    {
        if (Players.Count > 1) { isSinglePlayer = false; }
        isGameInProgress = true;
        SetupUnitDict();
        Debug.Log($"StartMission ==> chapter:{chapter} mission:{mission}");
        ServerChangeScene("Scene_Map_0" + chapter);
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

        Players.Add(player);

        player.SetDisplayName($"Player {Players.Count}");
        player.SetTeamColor(teamsColor[Players.Count-1]);
        player.SetPlayerID(Players.Count - 1);
        player.SetEnemyID(player.GetPlayerID() == 0 ? 1 : 0);
        player.SetTeamEnemyColor(teamsColor[player.GetEnemyID()]);
        player.SetPartyOwner(Players.Count == 1);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (SceneManager.GetActiveScene().name.StartsWith("Scene_Map"))
        {
            GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);
            GameBoardHandler gameBoardHandlerInstance = Instantiate(gameBoardHandlerPrefab);
            GreatWallController greatWallInstance = Instantiate(greatWallPrefab, gameBoardHandlerInstance.middleLinePoint.position, Quaternion.identity);
            GameObject doorMiddleInstance = Instantiate(doorPrefab, gameBoardHandlerInstance.middleDoorPoint.position, Quaternion.identity);
            GameObject doorLeftInstance = Instantiate(doorPrefab, gameBoardHandlerInstance.leftDoorPoint.position, Quaternion.identity);
            GameObject doorRightInstance = Instantiate(doorPrefab, gameBoardHandlerInstance.rightDoorPoint.position, Quaternion.identity);

            NetworkServer.Spawn(gameBoardHandlerInstance.gameObject);
            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);
            NetworkServer.Spawn(greatWallInstance.gameObject);
            NetworkServer.Spawn(doorMiddleInstance.gameObject);
            NetworkServer.Spawn(doorLeftInstance.gameObject);
            NetworkServer.Spawn(doorRightInstance.gameObject);

            foreach (RTSPlayer player in Players)
            {
                SetupUnitFactory(new Vector3(0,0,0), player);
                StartCoroutine(loadMilitary(0.1f, player, gameBoardHandlerInstance, Quaternion.identity));
            }
        }
    }
    private void SetupUnitFactory(Vector3 pos, RTSPlayer player)
    {
        GameObject factoryInstance = Instantiate(
                   unitFactoryPrefab,
                   pos,
                   Quaternion.identity);
        NetworkServer.Spawn(factoryInstance, player.connectionToClient);
    }
    private IEnumerator loadMilitary(float waitTime, RTSPlayer player, GameBoardHandler gameBoardHandlerInstance, Quaternion rotation)
    {
        yield return LoadUserTeam(player.GetUserID() , player.GetPlayerID() );
        yield return new WaitForSeconds(waitTime);
        JSONNode userTeamCard;
        int spawnCount = 1;
        string userkey = player.GetUserID() + "_" + player.GetPlayerID();
        //Debug.Log($"Userid {player.GetUserID()}, Team {teams}");
        for (int i = 0; i < userTeamDict[userkey].Count ; i++) {

            userTeamCard = userTeamDict[userkey][i];
            player.SetRace(userTeamCard["race"]);
            
            spawnCount = 1;
            UnitMeta.UnitKey unitKey = (UnitMeta.UnitKey) Enum.Parse(typeof(UnitMeta.UnitKey), userTeamCard["cardkey"]);
            //Debug.Log($"unitKey {unitKey}");
            while (spawnCount > 0)
            {
                GameObject spawnPointObject = gameBoardHandlerInstance.GetSpawnPointObject(UnitMeta.KeyType[unitKey], player.GetPlayerID());
                Vector3 spawnPosition = spawnPointObject.transform.position;
                //Debug.Log($"loadMilitary {unitType} spawnPosition {spawnPosition}");
                GameObject unit = Instantiate(unitDict[unitKey], spawnPosition, rotation) as GameObject;
                unit.GetComponent<Unit>().SetSpawnPointIndex(spawnPointObject.GetComponent<SpawnPoint>().spawnPointIndex);
                //unit.GetComponent<UnitPowerUp>().ServerTag(unit, player.GetPlayerID(), unitKey.ToString(), 1, userTeamCard["level"], userTeamCard["health"], userTeamCard["attack"], userTeamCard["repeatattackdelay"], userTeamCard["speed"], userTeamCard["defense"], userTeamCard["special"], userTeamCard["specialkey"], userTeamCard["passivekey"]);
                //unit.GetComponent<UnitPowerUp>().ServerPowerUp(unit, 1, userTeamCard["level"], userTeamCard["health"], userTeamCard["attack"], userTeamCard["repeatattackdelay"], userTeamCard["speed"], userTeamCard["defense"], userTeamCard["special"], userTeamCard["specialkey"], userTeamCard["passivekey"]);
                unit.name = unitKey.ToString();
                NetworkServer.Spawn(unit, player.connectionToClient);
                //Debug.Log("loadMilitary");
                //unit.GetComponent<UnitBody>().ServerChangeUnitRenderer(unit, player.GetPlayerID(), 1);
                spawnCount--;
            }
        }
    }
    private void SetupUnitDict()
    {
        unitDict.Clear();
        unitDict.Add(UnitMeta.UnitKey.UNDEADARCHER, undeadArcherPrefab);
        unitDict.Add(UnitMeta.UnitKey.HERO, heroPrefab);
        unitDict.Add(UnitMeta.UnitKey.KNIGHT, knightPrefab);
        unitDict.Add(UnitMeta.UnitKey.SPEARMAN, spearmanPrefab);
        unitDict.Add(UnitMeta.UnitKey.MINISKELETON, miniSkeletonPrefab);
        unitDict.Add(UnitMeta.UnitKey.GIANT, giantPrefab);
        unitDict.Add(UnitMeta.UnitKey.MAGE, magePrefab);
        unitDict.Add(UnitMeta.UnitKey.CAVALRY, cavalryPrefab);
        unitDict.Add(UnitMeta.UnitKey.KING, kingPrefab);
        unitDict.Add(UnitMeta.UnitKey.UNDEADHERO, undeadHeroPrefab);
        unitDict.Add(UnitMeta.UnitKey.ARCHER, archerPrefab);
        unitDict.Add(UnitMeta.UnitKey.RIDER, riderPrefab);
        unitDict.Add(UnitMeta.UnitKey.UNDEADKING, undeadKingPrefab);
        unitDict.Add(UnitMeta.UnitKey.UNDEADQUEEN, undeadQueenPrefab);
        unitDict.Add(UnitMeta.UnitKey.ODIN, odinPrefab);
        unitDict.Add(UnitMeta.UnitKey.THOR, thorPrefab);
        unitDict.Add(UnitMeta.UnitKey.LOKI, lokiPrefab);
        unitDict.Add(UnitMeta.UnitKey.ELFQUEEN, elfQueenPrefab);
        unitDict.Add(UnitMeta.UnitKey.ELFDEMONHUNTER, elfDemonHunterPrefab);
        unitDict.Add(UnitMeta.UnitKey.ELFTREEANT, elfTreeantPrefab);

    }

    #endregion

    #region Client

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        ClientOnConnected?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        ClientOnDisconnected?.Invoke();
    }

    public override void OnStopClient()
    {
        UnityEngine.Debug.Log($"RTS Network Manager OnStopClient ============================== ");
        Players.Clear();
    }
    #endregion
}