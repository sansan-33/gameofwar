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

    public Dictionary<UnitMeta.UnitType, GameObject> unitDict = new Dictionary<UnitMeta.UnitType, GameObject>();

    [SerializeField]
    private float spawnInterval = 60000f;
    private int spawnMoveRange = 1;

    private int initArcherCount = 0;
    private int initFootmanCount = 0;
    private int initKnightCount = 0;
    private int initHeroCount = 0;
    private List<int> lastPlayerSpawnPoint = new List<int> {0,1};
    private List<int> lastEnemySpawnPoint = new List<int> {3,4};
    private int spawnPointIndex = 0;

    [SerializeField] private float fireRate = 6000f;
   
    public override void OnStartClient()
    {
        initUnitDict();
    }
    public override void OnStartServer()
    {
        initUnitDict();
    }
    private void Update()
    {
       
    }
    [Command]
    public void CmdSpawnUnitWithPos(UnitMeta.UnitType unitType, int star, int playerID, bool spawnAuthority, Color teamColor, Vector3 spawnPosition, Vector3 targetPosition)
    {
        int unitsize = 1;
        if (UnitMeta.UnitSize.TryGetValue(unitType, out int value)) { unitsize = value; }
        Quaternion unitRotation = Quaternion.LookRotation(targetPosition - spawnPosition);

        StartCoroutine(ServerSpwanUnit(0.1f, playerID, spawnPosition, unitDict[unitType], unitType.ToString(), unitsize, spawnAuthority, star, teamColor, unitRotation));
    }
    [Command]
    public void CmdSpawnUnit(UnitMeta.UnitType unitType, int star, int playerID, bool spawnAuthority, Color teamColor)
    {
        //Vector3 spawnPosition = spawnPoints[playerID].position;
        //TODO spwan position should be based on leader / hero
        int spawnPoint = 0;
        if (playerID == 0)
            spawnPoint = lastPlayerSpawnPoint[spawnPointIndex++ % 2];
        else
            spawnPoint = lastEnemySpawnPoint[spawnPointIndex++ % 2];
        Vector3 spawnPosition = NetworkManager.startPositions[spawnPoint].position;
        int unitsize = 1;
        if (UnitMeta.UnitSize.TryGetValue(unitType, out int value)) { unitsize = value; }
        StartCoroutine(ServerSpwanUnit(0.1f, playerID, spawnPosition, unitDict[unitType], unitType.ToString(), unitsize, spawnAuthority, star, teamColor, Quaternion.identity));
    }
    [Server]
    private IEnumerator ServerSpwanUnit(float waitTime, int playerID, Vector3 spawnPosition, GameObject unitPrefab, string unitName, int spawnCount, bool spawnAuthority, int star, Color teamColor, Quaternion rotation)
    {
        yield return new WaitForSeconds(waitTime);
        while (spawnCount > 0)
        {
            Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
            spawnOffset.y = spawnPosition.y;
            GameObject unit = Instantiate(unitPrefab, spawnPosition + spawnOffset, rotation) as GameObject;
            unit.name = unitName;
            unit.tag = "Player" + playerID;
            
            powerUp(unit , star);
            // Cannot remove this one otherwise Tactical Behavior error
            //if(spawnAuthority)
            //Debug.Log($" ServerSpwanUnit Player ID {playerID} {unitName}");

            NetworkServer.Spawn(unit, connectionToClient);
            RpcTag(unit, playerID, unitName, star, teamColor);
           
            spawnCount--;
        }
    }
    public GameObject powerUp(GameObject unit , int star)
    {
   
        unit.GetComponent<Health>().ScaleMaxHealth(star);
       
        if (star == 1)
        {
            unit.GetComponent<IAttack>().ScaleDamageDeal(star);
        }
        else
        {
            unit.GetComponent<IAttack>().ScaleDamageDeal((star - 1) * 4);
        }
        
        unit.GetComponentInChildren<IBody>().SetRenderMaterial(star);
        //unit.GetComponentInChildren<IBody>().SetUnitSize(star);
        RpcPowerUp(unit, star);
        return unit;
    }
    public void Transform(GameObject Cavalry, GameObject Knight)
    {
        Cavalry.SetActive(false);
        Knight.SetActive(true);
    }
    [ClientRpc]
    void rpcTransform(GameObject Cavalry, GameObject Spearman)
    {
        Transform(Cavalry, Spearman);
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

    }
    [ClientRpc]
    void RpcTag(GameObject unit, int playerID, string unitName, int star, Color teamColor)
    {
        unit.name = unitName;
        unit.tag = "Player" + playerID;
        //Debug.Log($"RpcTag color is {teamColor}");
        unit.GetComponent<HealthDisplay>().SetHealthBarColor(teamColor);
    }
    [ClientRpc]
    void RpcPowerUp(GameObject unit , int star)
    {
        
        powerUp(unit, star);
    }
}