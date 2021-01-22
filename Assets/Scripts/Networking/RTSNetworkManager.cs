using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class RTSNetworkManager : NetworkManager
{
    [SerializeField] private GameObject unitBasePrefab = null;
    [SerializeField] private GameObject archerPrefab = null;
    [SerializeField] private GameObject knightPrefab = null;
    [SerializeField] private GameObject heroPrefab = null;
    [SerializeField] private GameObject spearmanPrefab = null;
    [SerializeField] private GameObject sampleUnitPrefab = null;

    [SerializeField] private GameOverHandler gameOverHandlerPrefab = null;

    public static event Action ClientOnConnected;
    public static event Action ClientOnDisconnected;

    private bool isGameInProgress = false;

    public List<RTSPlayer> Players { get; } = new List<RTSPlayer>();

    private int spawnMoveRange = 1;

    private Dictionary<Unit.UnitType, int> militaryList = new Dictionary<Unit.UnitType, int>();
    private Dictionary<Unit.UnitType, GameObject> unitDict = new Dictionary<Unit.UnitType, GameObject>();
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

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        Players.Clear();

        isGameInProgress = false;
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
        unitDict.Add(Unit.UnitType.SAMPLE, sampleUnitPrefab);


        ServerChangeScene("Scene_Map_01");
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

        Players.Add(player);

        player.SetDisplayName($"Player {Players.Count}");

        player.SetTeamColor(new Color(
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f)
        ));
        player.SetPlayerID(Players.Count - 1);
        player.SetEnemyID(player.GetPlayerID() == 0 ? 1 : 0);
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
                militaryList.Clear();
                if (player.GetPlayerID() == 0)
                {
                    militaryList.Add(Unit.UnitType.ARCHER, 2);
                    militaryList.Add(Unit.UnitType.KNIGHT, 0);
                    militaryList.Add(Unit.UnitType.HERO, 1);
                    militaryList.Add(Unit.UnitType.SAMPLE, 0);
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
        }else if (SceneManager.GetActiveScene().name.StartsWith("Scene_Testing_01"))
        {
         
            foreach (RTSPlayer player in Players)
            {
                Vector3 pos = GetStartPosition().position;
                SetupBase(pos, player);
                militaryList.Clear();
                militaryList.Add(Unit.UnitType.SAMPLE, 3);
                //militaryList.Add(Unit.UnitType.ARCHER, 2);
                //militaryList.Add(Unit.UnitType.HERO, 1);
                foreach (Unit.UnitType unitType in militaryList.Keys)
                {
                    StartCoroutine(loadMilitary(0.1f, player, pos, UnitDictionary.unitDict[unitType], unitType.ToString(), militaryList[unitType]));
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
        //baseInstance.tag = "PlayerBase" + player.GetPlayerID();
        NetworkServer.Spawn(baseInstance, player.connectionToClient);
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
        Players.Clear();
    }

    #endregion
}