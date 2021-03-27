using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Mirror;
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
    [SerializeField] private GameObject archerPrefab = null;

    [SerializeField] private GameOverHandler gameOverHandlerPrefab = null;
    [SerializeField] private GameBoardHandler gameBoardHandlerPrefab = null;

    public static event Action ClientOnConnected;
    public static event Action ClientOnDisconnected;
   
    private bool isGameInProgress = false;
    private bool isSinglePlayer = true;

    private List<Color> teamsColor = new List<Color>() { new Color(0f,0.6f,1f), new Color(1f,0f,0f)};
    public List<RTSPlayer> Players { get; } = new List<RTSPlayer>();

    private string urladdress = "http://192.168.2.181:8400";
    private int spawnMoveRange = 1;

    private Dictionary<UnitMeta.UnitKey, int> militaryList = new Dictionary<UnitMeta.UnitKey, int>();
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
        webReq.url = string.Format("{0}/{1}/{2}", urladdress, "gameserver/quit", port);
        webReq.method = "put";
        webReq.SendWebRequest();
        yield return new WaitForSeconds(10f);
        //Application.Quit();
    }
    public void StartGame()
    {
        if (Players.Count > 1) { isSinglePlayer = false; }
       
        isGameInProgress = true;

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

        ServerChangeScene("Scene_Map_04");
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

            NetworkServer.Spawn(gameBoardHandlerInstance.gameObject);
            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);

            foreach (RTSPlayer player in Players)
            {
                SetupUnitFactory(new Vector3(0,0,0), player);

                militaryList.Clear();
                if (player.GetPlayerID() == 0)
                {
                    militaryList.Add(UnitMeta.UnitKey.HERO, 2);
                    //militaryList.Add(UnitMeta.UnitKey.ARCHER, 1);
                    //militaryList.Add(UnitMeta.UnitKey.CAVALRY, 1);
                    //militaryList.Add(UnitMeta.UnitKey.SPEARMAN, 1);

                    StartCoroutine(loadMilitary(0.1f, player, gameBoardHandlerInstance, UnitMeta.UnitKey.KING, 1 , Quaternion.identity));
                }
                else
                {
                    //militaryList.Add(UnitMeta.UnitKey.MINISKELETON, 1);
                    militaryList.Add(UnitMeta.UnitKey.UNDEADHERO, 2);
                    //militaryList.Add(UnitMeta.UnitKey.UNDEADARCHER, 1);
                    //militaryList.Add(UnitMeta.UnitKey.RIDER, 1);
                    StartCoroutine(loadMilitary(0.1f, player, gameBoardHandlerInstance, UnitMeta.UnitKey.UNDEADKING, 1, Quaternion.Euler(0, 180,0)));
                }
                foreach (UnitMeta.UnitKey unitKey in militaryList.Keys)
                {
                    StartCoroutine(loadMilitary(0.1f, player, gameBoardHandlerInstance, unitKey, militaryList[unitKey], Quaternion.identity));
                }
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
    private IEnumerator loadMilitary(float waitTime, RTSPlayer player, GameBoardHandler gameBoardHandlerInstance, UnitMeta.UnitKey unitKey  , int spawnCount, Quaternion rotation)
    {

        yield return new WaitForSeconds(waitTime);
        while (spawnCount > 0)
        {
            GameObject spawnPointObject = gameBoardHandlerInstance.GetSpawnPointObject(UnitMeta.KeyType[unitKey], player.GetPlayerID());
            Vector3 spawnPosition = spawnPointObject.transform.position;
            //Debug.Log($"loadMilitary {unitType} spawnPosition {spawnPosition}");
            GameObject unit = Instantiate(unitDict[unitKey], spawnPosition, rotation) as GameObject;
            unit.GetComponent<Unit>().SetSpawnPointIndex(spawnPointObject.GetComponent<SpawnPoint>().spawnPointIndex);
            unit.name = unitKey.ToString();
            //unit.tag = "Player" + player.GetPlayerID();
            //unit.GetComponent<HealthDisplay>().SetHealthBarColor(player.GetTeamColor());
            NetworkServer.Spawn(unit, player.connectionToClient);
            //Debug.Log("loadMilitary");
            //unit.GetComponent<UnitBody>().ServerChangeUnitRenderer(unit, player.GetPlayerID(), 1);
            spawnCount--;
        }
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