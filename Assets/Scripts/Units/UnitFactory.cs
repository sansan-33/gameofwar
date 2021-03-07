using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;

public class UnitFactory : NetworkBehaviour
{
    [SerializeField] private GameObject archerPrefab = null;
    [SerializeField] private GameObject knightPrefab = null;
    [SerializeField] private GameObject magePrefab = null;
    [SerializeField] private GameObject cavalryPrefab = null;
    [SerializeField] private GameObject heroPrefab = null;
    [SerializeField] private GameObject spearmanPrefab = null;
    [SerializeField] private GameObject miniSkeletonUnitPrefab = null;
    [SerializeField] private GameObject giantUnitPrefab = null;
    [SerializeField] private GameObject kingPrefab = null;
    [SerializeField] private GameObject undeadHeroPrefab = null;
    [SerializeField] private GameBoardHandler gameBoardHandlerPrefab = null;

    public Dictionary<UnitMeta.UnitType, GameObject> unitDict = new Dictionary<UnitMeta.UnitType, GameObject>();

    [SerializeField]
    private float spawnInterval = 60000f;
    private int spawnMoveRange = 1;

    private int initArcherCount = 0;
    private int initFootmanCount = 0;
    private int initHeroCount = 0;
    private List<int> lastPlayerSpawnPoint = new List<int> {0,1};
    private List<int> lastEnemySpawnPoint = new List<int> {3,4};
    private int spawnPointIndex = 0;

    [SerializeField] private float fireRate = 6000f;

    public override void OnStartClient()
    {
        initUnitDict();
        if (gameBoardHandlerPrefab == null)
        {
            foreach (GameObject board in GameObject.FindGameObjectsWithTag("GameBoardSystem"))
            {
                gameBoardHandlerPrefab = board.GetComponent<GameBoardHandler>();
            }
        }
    }
    public override void OnStartServer()
    {
        initUnitDict();
        if (gameBoardHandlerPrefab == null)
        {
            foreach (GameObject board in GameObject.FindGameObjectsWithTag("GameBoardSystem"))
            {
                gameBoardHandlerPrefab = board.GetComponent<GameBoardHandler>();
            }
        }
    }
    private void Update()
    {
       
    }
    [Command]
    public void CmdSpawnUnitRotation(UnitMeta.UnitType unitType, int star, int playerID, bool spawnAuthority, Color teamColor,  Quaternion unitRotation)
    {
        if (!UnitMeta.UnitSize.TryGetValue(unitType, out int unitsize)) { unitsize = 1; }

        GameObject spawnPointObject = gameBoardHandlerPrefab.GetSpawnPointObject(unitType, playerID);
        Vector3 spawnPosition = spawnPointObject.transform.position;

        StartCoroutine(ServerSpwanUnit(0.1f, playerID, spawnPosition, unitDict[unitType], unitType.ToString(), unitsize, spawnAuthority, star, teamColor, unitRotation , spawnPointObject.GetComponent<SpawnPoint>().spawnPointIndex));
    }
    [Command]
    public void CmdSpawnUnit(UnitMeta.UnitType unitType, int star, int playerID, bool spawnAuthority, Color teamColor)
    {
        if (!UnitMeta.UnitSize.TryGetValue(unitType, out int unitsize)) { unitsize = 1; }

        GameObject spawnPointObject = gameBoardHandlerPrefab.GetSpawnPointObject(unitType, playerID);
        Vector3 spawnPosition = spawnPointObject.transform.position;

        StartCoroutine(ServerSpwanUnit(0.1f, playerID, spawnPosition, unitDict[unitType], unitType.ToString(), unitsize, spawnAuthority, star, teamColor, Quaternion.identity, spawnPointObject.GetComponent<SpawnPoint>().spawnPointIndex));

        //CmdSpawnUnitRotation(unitType, star, playerID, spawnAuthority, teamColor, Quaternion.identity);
    }
    [Server]
    private IEnumerator ServerSpwanUnit(float waitTime, int playerID, Vector3 spawnPosition, GameObject unitPrefab, string unitName, int spawnCount, bool spawnAuthority, int star, Color teamColor, Quaternion rotation, int spawnPointIndex)
    {
        yield return new WaitForSeconds(waitTime);
        while (spawnCount > 0)
        {
            Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
            spawnOffset.y = spawnPosition.y;
            GameObject unit = Instantiate(unitPrefab, spawnPosition + spawnOffset, rotation) as GameObject;
            unit.name = unitName;
            unit.tag = "Player" + playerID;
            //unit.GetComponent<Unit>().SetSpawnPointIndex(spawnPointIndex);

            // Cannot remove this one otherwise Tactical Behavior error
            //if(spawnAuthority)
            //Debug.Log($" ServerSpwanUnit Player ID {playerID} {unitName}");

            NetworkServer.Spawn(unit, connectionToClient);
            RpcTag(unit, playerID, unitName, star, teamColor, spawnPointIndex);
            unit.GetComponent<UnitPowerUp>().ServerPowerUp(unit, star);
            //unit.GetComponent<UnitPowerUp>().RpcPowerUp(unit, star);
            //Debug.Log($"unit.GetComponent<UnitPowerUp>().RpcPowerUp(unit, star){unit.GetComponent<UnitPowerUp>()}");
            spawnCount--;
        }
    }
    private void initUnitDict()
    {

        unitDict.Clear();
        unitDict.Add(UnitMeta.UnitType.ARCHER, archerPrefab);
        unitDict.Add(UnitMeta.UnitType.HERO, heroPrefab);
        unitDict.Add(UnitMeta.UnitType.KNIGHT, knightPrefab);
        unitDict.Add(UnitMeta.UnitType.SPEARMAN, spearmanPrefab);
        unitDict.Add(UnitMeta.UnitType.MAGE, magePrefab);
        unitDict.Add(UnitMeta.UnitType.CAVALRY, cavalryPrefab);
        unitDict.Add(UnitMeta.UnitType.MINISKELETON, miniSkeletonUnitPrefab);
        unitDict.Add(UnitMeta.UnitType.GIANT, giantUnitPrefab);
        unitDict.Add(UnitMeta.UnitType.KING, kingPrefab);
        unitDict.Add(UnitMeta.UnitType.UNDEADHERO, undeadHeroPrefab);

    }
    [ClientRpc]
    void RpcTag(GameObject unit, int playerID, string unitName, int star, Color teamColor, int spawnPointIndex)
    {
        unit.name = unitName;
        unit.tag = "Player" + playerID;
        //Debug.Log($"RpcTag color is {teamColor}");
        unit.GetComponent<HealthDisplay>().SetHealthBarColor(teamColor);
        unit.GetComponentInChildren<UnitBody>().ServerChangeUnitRenderer(unit,playerID, star);
        unit.GetComponent<Unit>().SetSpawnPointIndex(spawnPointIndex);
    }

}