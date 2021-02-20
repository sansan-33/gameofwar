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
    [SerializeField] private GameObject unitBasePrefab = null;
    [SerializeField] private GameObject archerPrefab = null;
    [SerializeField] private GameObject miniSkeletonPrefab = null;
    [SerializeField] private GameObject knightPrefab = null;
    [SerializeField] private GameObject heroPrefab = null;
    [SerializeField] private GameObject spearmanPrefab = null;
    [SerializeField] private GameObject sampleUnitPrefab = null;
    [SerializeField] private GameObject unitFactoryPrefab = null;
    [SerializeField] private GameObject giantPrefab = null;
    [SerializeField] private GameObject magePrefab = null;
    [SerializeField] private GameObject cavalryPrefab = null;

    [SerializeField] private GameOverHandler gameOverHandlerPrefab = null;

    public static event Action ClientOnConnected;
    public static event Action ClientOnDisconnected;
   
    private bool isGameInProgress = false;
    private List<Color> teamsColor = new List<Color>() { new Color(0f,0.6f,1f), new Color(1f,0f,0f)};
    public List<RTSPlayer> Players { get; } = new List<RTSPlayer>();

    private string urladdress = "http://192.168.2.181:8400";
    private int spawnMoveRange = 1;

    private Dictionary<Unit.UnitType, int> militaryList = new Dictionary<Unit.UnitType, int>();
    private Dictionary<Unit.UnitType, GameObject> unitDict = new Dictionary<Unit.UnitType, GameObject>();
   
    #region Server
    public override void OnServerConnect(NetworkConnection conn)
    {
        Debug.Log($"Server Connected ==============isGameInProgress {isGameInProgress} / Players {Players.Count}");
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
        Application.Quit();
    }
    public void StartGame()
    {
        //if (Players.Count < 2) { return; }
       
        isGameInProgress = true;

        unitDict.Clear();
        unitDict.Add(Unit.UnitType.ARCHER, archerPrefab);
        unitDict.Add(Unit.UnitType.HERO, heroPrefab);
        unitDict.Add(Unit.UnitType.KNIGHT, knightPrefab);
        unitDict.Add(Unit.UnitType.SPEARMAN, spearmanPrefab);
        unitDict.Add(Unit.UnitType.MINISKELETON, miniSkeletonPrefab);
        unitDict.Add(Unit.UnitType.GIANT, giantPrefab);
        unitDict.Add(Unit.UnitType.MAGE, magePrefab);
        unitDict.Add(Unit.UnitType.CAVALRY, cavalryPrefab);
        ServerChangeScene("Scene_Map_02");
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

            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);

            foreach (RTSPlayer player in Players)
            {
                Vector3 pos = GetStartPosition().position;
                //Debug.Log($"What is unitbase tag | {baseInstance.tag} | playerID |{player.GetPlayerID()}|  ? ");               
                SetupBase(pos, player);
                SetupBase(GetStartPosition().position, player);
                SetupUnitFactory(pos, player);
                militaryList.Clear();
                if (player.GetPlayerID() == 0)
                {
                    militaryList.Add(Unit.UnitType.HERO, 2);
                    //militaryList.Add(Unit.UnitType.SPEARMAN, 5);
                    //militaryList.Add(Unit.UnitType.GIANT, 1);
                    //militaryList.Add(Unit.UnitType.SPEARMAN, 1);
                    //militaryList.Add(Unit.UnitType.SPEARMAN, 1);
                    //militaryList.Add(Unit.UnitType.SAMPLEUNIT, 5);

                }
                else
                {
                    militaryList.Add(Unit.UnitType.SPEARMAN, 3);
                }
                foreach (Unit.UnitType unitType in militaryList.Keys)
                {
                    StartCoroutine(loadMilitary(0.1f, player, pos, unitDict[unitType], unitType.ToString(), militaryList[unitType]));
                }
            }
        }

    }

    private void SetupBase(Vector3 pos, RTSPlayer player)
    {
        GameObject baseInstance = Instantiate(
                   unitBasePrefab,
                   pos,
                   Quaternion.identity);
        baseInstance.SetActive(true);
        //The Tag will not be set in client machine
        NetworkServer.Spawn(baseInstance, player.connectionToClient);
    }
    private void SetupUnitFactory(Vector3 pos, RTSPlayer player)
    {
        GameObject factoryInstance = Instantiate(
                   unitFactoryPrefab,
                   pos,
                   Quaternion.identity);
        NetworkServer.Spawn(factoryInstance, player.connectionToClient);
    }

    private IEnumerator loadMilitary(float waitTime, RTSPlayer player, Vector3 spawnPosition, GameObject unitPrefab, string unitName, int spawnCount)
    {
        yield return new WaitForSeconds(waitTime);
        while (spawnCount > 0)
        {
            Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
            spawnOffset.y = spawnPosition.y;
            GameObject unit = Instantiate(unitPrefab, spawnPosition + spawnOffset, Quaternion.identity) as GameObject;
            unit.name = unitName;
            unit.tag = "Player" + player.GetPlayerID();
            unit.GetComponent<HealthDisplay>().SetHealthBarColor(player.GetTeamColor());
            NetworkServer.Spawn(unit, player.connectionToClient);
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