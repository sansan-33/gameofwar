using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;

public class UnitFactory : NetworkBehaviour
{
    // Human
    [SerializeField] private GameObject archerPrefab = null;
    [SerializeField] private GameObject knightPrefab = null;
    [SerializeField] private GameObject magePrefab = null;
    [SerializeField] private GameObject cavalryPrefab = null;
    [SerializeField] private GameObject heroPrefab = null;
    [SerializeField] private GameObject spearmanPrefab = null;
    [SerializeField] private GameObject kingPrefab = null;
    [SerializeField] private GameObject humanWallPrefab = null;

    // Undead
    [SerializeField] private GameObject miniSkeletonPrefab = null;
    [SerializeField] private GameObject giantPrefab = null;
    [SerializeField] private GameObject riderPrefab = null;
    [SerializeField] private GameObject undeadHeroPrefab = null;
    [SerializeField] private GameObject undeadArcherPrefab = null;
    [SerializeField] private GameObject undeadKingPrefab = null;
    [SerializeField] private GameObject undeadLichPrefab = null;

    // god
    [SerializeField] private GameObject godarcherPrefab = null;
    [SerializeField] private GameObject godknightPrefab = null;
    [SerializeField] private GameObject godmagePrefab = null;
    [SerializeField] private GameObject godcavalryPrefab = null;
    [SerializeField] private GameObject thorPrefab = null;
    [SerializeField] private GameObject lokiPrefab = null;
    [SerializeField] private GameObject godspearmanPrefab = null;
    [SerializeField] private GameObject odinPrefab = null;
    [SerializeField] private GameObject godwallPrefab = null;

    [SerializeField] private GameBoardHandler gameBoardHandlerPrefab = null;

    public Dictionary<UnitMeta.UnitKey, GameObject> unitDict = new Dictionary<UnitMeta.UnitKey, GameObject>();

    [SerializeField]
    private int spawnMoveRange = 1000;
   
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
    [Command]
    public void CmdSpawnTeamUnit(UnitMeta.UnitKey unitKey, int star, int playerID, int cardLevel, int health, int attack, float repeatAttackDelay, int speed, int defense, int special, string specialkey, string passivekey, Color teamColor, Quaternion unitRotation)
    {
        if (!UnitMeta.UnitSize.TryGetValue(UnitMeta.KeyType[unitKey] , out int unitsize)) { unitsize = 1; }

        GameObject spawnPointObject = gameBoardHandlerPrefab.GetSpawnPointObject(UnitMeta.KeyType[unitKey], playerID);
        Vector3 spawnPosition = spawnPointObject.transform.position;

        StartCoroutine(ServerSpwanUnit(playerID, spawnPosition, unitDict[unitKey], UnitMeta.KeyType[unitKey].ToString(), unitsize, cardLevel, health, attack, repeatAttackDelay, speed, defense, special, specialkey, passivekey, star, teamColor, unitRotation, spawnPointObject.GetComponent<SpawnPoint>().spawnPointIndex));
    }
    [Command]
    public void CmdSpawnUnitRotation(UnitMeta.Race race, UnitMeta.UnitType unitType, int star, int playerID,int cardLevel, int health, int attack, float repeatAttackDelay, int speed, int defense, int special, string specialkey, string passivekey, Color teamColor,  Quaternion unitRotation)
    {
        if (!UnitMeta.UnitSize.TryGetValue(unitType, out int unitsize)) { unitsize = 1; }

        GameObject spawnPointObject = gameBoardHandlerPrefab.GetSpawnPointObject(unitType, playerID);
        Vector3 spawnPosition = spawnPointObject.transform.position;

        StartCoroutine(ServerSpwanUnit(playerID, spawnPosition, unitDict[UnitMeta.UnitRaceTypeKey[race][unitType]], unitType.ToString(), unitsize, cardLevel, health, attack, repeatAttackDelay, speed, defense, special, specialkey, passivekey , star, teamColor, unitRotation , spawnPointObject.GetComponent<SpawnPoint>().spawnPointIndex));
    }
    [Command]
    public void CmdSpawnUnit(UnitMeta.Race race, UnitMeta.UnitType unitType, int star, int playerID,int cardLevel, int health, int attack, float repeatAttackDelay, int speed, int defense, int special, string specialkey, string passivekey, Color teamColor)
    {
        if (!UnitMeta.UnitSize.TryGetValue(unitType, out int unitsize)) { unitsize = 1; }

        GameObject spawnPointObject = gameBoardHandlerPrefab.GetSpawnPointObject(unitType, playerID);
        Vector3 spawnPosition = spawnPointObject.transform.position;
        StartCoroutine(ServerSpwanUnit(playerID, spawnPosition, unitDict[UnitMeta.UnitRaceTypeKey[race][unitType]], unitType.ToString(), unitsize, cardLevel, health, attack, repeatAttackDelay, speed, defense, special, specialkey, passivekey, star, teamColor, Quaternion.identity, spawnPointObject.GetComponent<SpawnPoint>().spawnPointIndex));
    }
    [Server]
    private IEnumerator ServerSpwanUnit(int playerID, Vector3 spawnPosition, GameObject unitPrefab, string unitName, int spawnCount,int cardLevel, int health, int attack, float repeatAttackDelay, int speed, int defense, int special, string specialkey, string passivekey ,int star, Color teamColor, Quaternion rotation, int spawnPointIndex)
    {
        //float waitTime = 0.1f;
        int spawnCountOffset = 0;
        //yield return new WaitForSeconds(waitTime);
        while (spawnCount > 0)
        {
            Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange * spawnCountOffset;
            spawnOffset.y = 0;
            spawnOffset.z = 0;
            GameObject unit = Instantiate(unitPrefab, spawnPosition + spawnOffset, rotation) as GameObject;
            //Debug.Log($"Unit {unitName} Spawn position {spawnPosition} + spawnOffset  {spawnOffset} = unit position {spawnPosition + spawnOffset}");
            NetworkServer.Spawn(unit, connectionToClient);
            if (unit.GetComponent<Unit>().unitType != UnitMeta.UnitType.WALL)
            {
                unit.GetComponent<UnitPowerUp>().PowerUp(playerID, unitName, spawnPointIndex, star, cardLevel, health, attack, repeatAttackDelay, speed, defense, special, specialkey, passivekey);
            }
            //Debug.Log($"unit.GetComponent<UnitPowerUp>().RpcPowerUp(unit, star){unit.GetComponent<UnitPowerUp>()}");
            spawnCount--;
            spawnCountOffset += 2;
        }
        yield return null;
    }
    [Command]
    public void CmdDropUnit(int playerID, Vector3 spawnPosition, UnitMeta.Race race, UnitMeta.UnitType unitType,string unitName, int spawnCount,int cardLevel, int health, int attack, float repeatAttackDelay, int speed, int defense, int special,string specialkey, string passivekey , int star, Color teamColor, Quaternion rotation)
    {
        GameObject spawnPointObject = gameBoardHandlerPrefab.GetSpawnPointObject(unitType, playerID);
        StartCoroutine(ServerSpwanUnit(playerID, spawnPosition, unitDict[UnitMeta.UnitRaceTypeKey[race][unitType]], unitName, spawnCount, cardLevel, health, attack, repeatAttackDelay, speed, defense, special , specialkey, passivekey , star, teamColor, rotation, spawnPointObject.GetComponent<SpawnPoint>().spawnPointIndex));
    }
    public void initUnitDict()
    {
        unitDict.Clear();
        unitDict.Add(UnitMeta.UnitKey.ARCHER, archerPrefab);
        unitDict.Add(UnitMeta.UnitKey.HERO, heroPrefab);
        unitDict.Add(UnitMeta.UnitKey.KNIGHT, knightPrefab);
        unitDict.Add(UnitMeta.UnitKey.SPEARMAN, spearmanPrefab);
        unitDict.Add(UnitMeta.UnitKey.MAGE, magePrefab);
        unitDict.Add(UnitMeta.UnitKey.CAVALRY, cavalryPrefab);
        unitDict.Add(UnitMeta.UnitKey.KING, kingPrefab);
        unitDict.Add(UnitMeta.UnitKey.MINISKELETON, miniSkeletonPrefab);
        unitDict.Add(UnitMeta.UnitKey.GIANT, giantPrefab);
        unitDict.Add(UnitMeta.UnitKey.UNDEADHERO, undeadHeroPrefab);
        unitDict.Add(UnitMeta.UnitKey.UNDEADARCHER, undeadArcherPrefab);
        unitDict.Add(UnitMeta.UnitKey.RIDER, riderPrefab);
        unitDict.Add(UnitMeta.UnitKey.LICH, undeadLichPrefab);
        unitDict.Add(UnitMeta.UnitKey.UNDEADKING, undeadKingPrefab);
        unitDict.Add(UnitMeta.UnitKey.HUMANWALL, humanWallPrefab);
        unitDict.Add(UnitMeta.UnitKey.GODARCHER, godarcherPrefab);
        unitDict.Add(UnitMeta.UnitKey.THOR, thorPrefab);
        unitDict.Add(UnitMeta.UnitKey.LOKI, lokiPrefab);
        unitDict.Add(UnitMeta.UnitKey.GODKNIGHT, godknightPrefab);
        unitDict.Add(UnitMeta.UnitKey.GODSPEARMAN, godspearmanPrefab);
        unitDict.Add(UnitMeta.UnitKey.GODMAGE, godmagePrefab);
        unitDict.Add(UnitMeta.UnitKey.GODCAVALRY, godcavalryPrefab);
        unitDict.Add(UnitMeta.UnitKey.ODIN, odinPrefab);
        unitDict.Add(UnitMeta.UnitKey.GODWALL, godwallPrefab);

    }
    public GameObject GetUnitPrefab(UnitMeta.Race race, UnitMeta.UnitType unitType)
    {
        return unitDict[UnitMeta.UnitRaceTypeKey[race][unitType]];
    }
    public GameObject GetUnitPrefab(UnitMeta.UnitKey unitKey)
    {
        return unitDict[unitKey];
    }
}