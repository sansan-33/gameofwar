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

    public Dictionary<Unit.UnitType, GameObject> unitDict = new Dictionary<Unit.UnitType, GameObject>();

    [SerializeField]
    private float spawnInterval = 60000f;
    private int spawnMoveRange = 1;

    private int initArcherCount = 0;
    private int initFootmanCount = 0;
    private int initKnightCount = 0;
    private int initHeroCount = 0;

    [SerializeField] private float fireRate = 6000f;
   
    public override void OnStartClient()
    {
        //Debug.Log("Unit Factory Initialize the unitDic");
        unitDict.Clear();
        unitDict.Add(Unit.UnitType.ARCHER, archerPrefab);
        unitDict.Add(Unit.UnitType.HERO, heroPrefab);
        unitDict.Add(Unit.UnitType.KNIGHT, knightPrefab);
        unitDict.Add(Unit.UnitType.SPEARMAN, spearmanPrefab);
        unitDict.Add(Unit.UnitType.MAGE, magePrefab);
        unitDict.Add(Unit.UnitType.CAVALRY, cavalryPrefab);
        unitDict.Add(Unit.UnitType.MINISKELETON, miniSkeletonUnitPrefab);
        unitDict.Add(Unit.UnitType.GIANT, giantUnitPrefab);
    }
    private void Update()
    {
       
    }
    [Command]
    public void CmdSpawnUnit(Unit.UnitType unitType, int star, int playerID, bool spawnAuthority)
    {
        //Debug.Log($" CmdSpawnUnit Player ID {playerID} ");
        Vector3 spawnPosition = GameObject.FindGameObjectWithTag("PlayerBase" + playerID ).transform.position ;
        int unitsize = 1;
        if(Unit.UnitSize.TryGetValue(unitType , out int value)){ unitsize = value; }
        StartCoroutine(ServerSpwanUnit(0.1f, playerID, spawnPosition , unitDict[unitType], unitType.ToString(), unitsize, spawnAuthority , star ));
        
    }
    [Server]
    private IEnumerator ServerSpwanUnit(float waitTime, int playerID, Vector3 spawnPosition, GameObject unitPrefab, string unitName, int spawnCount, bool spawnAuthority, int star )
    {
        yield return new WaitForSeconds(waitTime);
        while (spawnCount > 0)
        {
            Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
            spawnOffset.y = spawnPosition.y;
            GameObject unit = Instantiate(unitPrefab, spawnPosition + spawnOffset, Quaternion.identity) as GameObject;
            unit.name = unitName;
            unit.tag = "Player" + playerID;
            unit = powerUp(unit , star);
            // Cannot remove this one otherwise Tactical Behavior error
            //if(spawnAuthority)
                NetworkServer.Spawn(unit, connectionToClient);
            
            spawnCount--;
        }
    }
    private GameObject powerUp(GameObject unit , int star)
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

        return unit;
    }   
}