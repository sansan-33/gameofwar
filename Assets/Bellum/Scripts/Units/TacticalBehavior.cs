
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using BehaviorDesigner.Runtime;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using Debug = UnityEngine.Debug;

public class TacticalBehavior : MonoBehaviour
{
    [SerializeField] List<GameObject> PlayerEnemyGroup = new List<GameObject>();
    public static event Action UnitTagUpdated ;
    private RTSPlayer player;
    private int PLAYERID = 0;
    private int ENEMYID = 0;
    private bool usedTaticalAttack = false;

    private Dictionary<int, List<BehaviorTree>> leaderPlayerBehaviorTreeGroup = new Dictionary<int, List<BehaviorTree>>();
    private Dictionary<int, List<BehaviorTree>> leaderEnemyBehaviorTreeGroup = new Dictionary<int, List<BehaviorTree>>();
    private Dictionary<int, Dictionary<int, List<BehaviorTree>>> playerBehaviorTreeGroup = new Dictionary<int, Dictionary<int, List<BehaviorTree>>>();
    private Dictionary<int, Dictionary<int, List<BehaviorTree>>> enemyBehaviorTreeGroup = new Dictionary<int, Dictionary<int, List<BehaviorTree>>>();

    private Dictionary<int, Dictionary<int, Dictionary<int, List<BehaviorTree>>>> behaviorTreeGroups = new Dictionary<int, Dictionary < int, Dictionary<int, List<BehaviorTree>>>>();

    public enum BehaviorSelectionType { Attack, Charge, MarchingFire, Flank, Ambush, ShootAndScoot, Leapfrog, Surround, Defend, Hold, Retreat, Reinforcements, Last };
    public enum TaticalAttack { SPINATTACK, CAVALRYCHARGES, ARROWRAIN, ABSOLUTEDEFENSE, OFF  };
    private TaticalAttack[] TaticalAttackCurrent = { TaticalAttack.OFF, TaticalAttack.OFF };

    private Dictionary<int, Dictionary<int, GameObject>> leaders = new Dictionary<int, Dictionary<int, GameObject>>();
    private Dictionary<int, Dictionary<int,  List<BehaviorSelectionType>>> leaderTacticalType = new Dictionary<int, Dictionary<int, List<BehaviorSelectionType>>>();
    private int selectedLeaderId = 0;
    private int selectedEnemyLeaderId = 0;
    private GameBoardHandler gameBoardHandlerPrefab = null;
    private Dictionary<int, GameObject> KINGBOSS = new Dictionary<int, GameObject>();
    private float newRadius = 0.1f;
    private float newDefendRadius = 12f;
    private Dictionary<UnitMeta.UnitType, TacticalBehavior.BehaviorSelectionType> unitTacticalPlayer = UnitMeta.DefaultUnitTactical;
    private Dictionary<UnitMeta.UnitType, TacticalBehavior.BehaviorSelectionType> unitTacticalEnemey = UnitMeta.DefaultUnitTactical;
    private Dictionary<int, Dictionary<UnitMeta.UnitType, TacticalBehavior.BehaviorSelectionType>> unitTactical = new Dictionary<int, Dictionary<UnitMeta.UnitType, TacticalBehavior.BehaviorSelectionType>>();
    private Dictionary<int, bool> ISGATEOPENED = new Dictionary<int, bool>();

    #region Client

    public void Start()
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        PLAYERID = player.GetPlayerID();
        ENEMYID = player.GetEnemyID();
        unitTactical[PLAYERID] = unitTacticalPlayer;
        unitTactical[ENEMYID] = unitTacticalEnemey;
        ISGATEOPENED[PLAYERID] = false;
        ISGATEOPENED[ENEMYID] = false;
        StartCoroutine(AssignTag());

        behaviorTreeGroups.Add(PLAYERID, playerBehaviorTreeGroup);
        behaviorTreeGroups.Add(ENEMYID, enemyBehaviorTreeGroup);
        leaderTacticalType.Add(PLAYERID, new Dictionary<int, List<BehaviorSelectionType>>() );
        leaderTacticalType.Add(ENEMYID, new Dictionary<int, List<BehaviorSelectionType>>() );
        leaders.Add(PLAYERID, new Dictionary<int, GameObject>());
        leaders.Add(ENEMYID, new Dictionary<int, GameObject>());
  
        //Make sure if enemy spawned, need to update TB formation for new target
        Unit.ClientOnUnitSpawned += TryReinforce;
        Unit.ClientOnUnitDespawned += TryReinforce;
        //LeaderScrollList.LeaderSelected += HandleLeaderSelected;
        GameOverHandler.ClientOnGameOver += HandleGameOver;
        UnitProjectile.GateOpened += SetGateOpen;
        UnitWeapon.GateOpened += SetGateOpen;
    }
    public void OnDestroy()
    {
        Unit.ClientOnUnitSpawned -= TryReinforce;
        Unit.ClientOnUnitDespawned -= TryReinforce;
        GameOverHandler.ClientOnGameOver -= HandleGameOver;
        //LeaderScrollList.LeaderSelected -= HandleLeaderSelected;
        UnitProjectile.GateOpened -= SetGateOpen;
        UnitWeapon.GateOpened -= SetGateOpen;
    }
    public IEnumerator AssignTag()
    {
        //Debug.Log("AssignTag");
        // Simulate RpcClientTag for player 1 
        // Need to Assing Tag for player 1 in multi player mode
        // Rpc Client Tag not implement in RTS Network Manager
        bool ISTAGGED = false;
        while (!ISTAGGED)
        {
            yield return new WaitForSeconds(0.1f);
    
            GameObject[] armies = GameObject.FindGameObjectsWithTag("Unit");
            foreach (GameObject army in armies)
            {
                if (army.TryGetComponent<Unit>(out Unit unit)){
                    if (unit.hasAuthority)
                    {
                        army.tag = "Player" + player.GetPlayerID();
                        if (unit.unitType == UnitMeta.UnitType.KING)
                        {
                            army.tag = "King" + player.GetPlayerID();
                            KINGBOSS[player.GetPlayerID()] = army;
                        }
                    }
                    else
                    {
                        //Only Assing Enemy Base Tag if mulitplayer
                        army.tag = "Player" + player.GetEnemyID();
                        if (unit.unitType == UnitMeta.UnitType.KING)
                        {
                            army.tag = "King" + player.GetEnemyID();
                            KINGBOSS[player.GetEnemyID()] = army;
                        }
                    }
                }
            }
            
            if (gameBoardHandlerPrefab == null)
            {
                foreach (GameObject board in GameObject.FindGameObjectsWithTag("GameBoardSystem"))
                {
                    gameBoardHandlerPrefab = board.GetComponent<GameBoardHandler>();
                    gameBoardHandlerPrefab.initPlayerGameBoard();
                }
            }
            yield return new WaitForSeconds(1f);
            //Debug.Log($"player0: {GameObject.FindGameObjectsWithTag("Player0").Length} / Player2 : {GameObject.FindGameObjectsWithTag("Player1").Length}");
            if ( GameObject.FindGameObjectsWithTag("Player0").Length > 0 && GameObject.FindGameObjectsWithTag("King0").Length > 0  && GameObject.FindGameObjectsWithTag("Player1").Length > 0 && GameObject.FindGameObjectsWithTag("King1").Length > 0)
            {
                ISTAGGED = true;
                UnitTagUpdated?.Invoke();
                yield return new WaitForSeconds(1f);
                yield return TacticalFormation(PLAYERID, ENEMYID,null);
            }

        }
        //Debug.Log("AssignTag ============================ END ");

    }
   
    public IEnumerator TacticalFormation(int playerid, int enemyid, GameObject unit)
    {
        yield return new WaitForSeconds(0.1f);
        if (gameBoardHandlerPrefab == null) { yield break; }

        //var stopwatch = new Stopwatch();
        //stopwatch.Start();
        bool provoke = false;
        GameObject defendObject;
      
        GameObject[] units = GameObject.FindGameObjectsWithTag("Player" + playerid);
        GameObject king = GameObject.FindGameObjectWithTag("King" + playerid);
        GameObject[] provokeTanks = GameObject.FindGameObjectsWithTag("Provoke" + playerid);
        GameObject[] sneakyFootman = GameObject.FindGameObjectsWithTag("Sneaky" + playerid);

        GameObject[] provokeEnemyTanks = GameObject.FindGameObjectsWithTag("Provoke" + enemyid);
        int enemyCount = GameObject.FindGameObjectsWithTag("Player" + enemyid).Length + GameObject.FindGameObjectsWithTag("Provoke" + enemyid).Length;
        int leaderUnitTypeID = 0;
        List<GameObject> armies = new List<GameObject>();
       
        if (unit == null) {
            leaders[playerid].Clear();
            behaviorTreeGroups[playerid].Clear();
            armies = units.ToList();
        } else {
            leaders[playerid].Remove((int)unit.GetComponent<Unit>().unitType);
            behaviorTreeGroups[playerid].Remove((int)unit.GetComponent<Unit>().unitType);
            foreach (GameObject child in units) {
                if (child.GetComponent<Unit>().unitType == unit.GetComponent<Unit>().unitType)
                    armies.Add(child);
            }
        }
        if (king != null)
            armies.Add(king);
        if (provokeTanks != null && provokeTanks.Length > 0)
            armies.AddRange(provokeTanks.ToList());
        if (sneakyFootman != null && sneakyFootman.Length > 0)
            armies.AddRange(sneakyFootman.ToList());
        if (provokeEnemyTanks != null && provokeEnemyTanks.Length > 0 )
            provoke = true;

        foreach (GameObject child in armies)
        {
            if (child == null) { continue; }

            leaderUnitTypeID = (int)child.GetComponent<Unit>().unitType;
        
            if (child.name.Contains("]"))
            {
                child.name = child.name.Substring(child.name.IndexOf("]") + 2 );
            }
            if (!leaders[playerid].ContainsKey(leaderUnitTypeID))
            {
                leaders[playerid].Add(leaderUnitTypeID, child);
                child.GetComponent<Unit>().isLeader = true;
                if(!child.name.Contains("*"))
                    child.name = "*" + child.name;
            }
            child.transform.parent = PlayerEnemyGroup[playerid].transform;
        }
        for (int j = 0; j < PlayerEnemyGroup[playerid].transform.childCount; ++j)
        {
            var child = PlayerEnemyGroup[playerid].transform.GetChild(j);
            leaderUnitTypeID = (int)child.GetComponent<Unit>().unitType;
      
            defendObject = gameBoardHandlerPrefab.GetSpawnPointObjectByIndex( (UnitMeta.UnitType) leaderUnitTypeID , playerid, child.GetComponent<Unit>().GetSpawnPointIndex());
            var agentTrees = child.GetComponents<BehaviorTree>();
            for (int k = 0; k < agentTrees.Length; ++k)
            {
                var group = agentTrees[k].Group;
                
                if ( group == 4 || group == 6 || group == 9 || group == 11 || group == 12) { continue; }
                agentTrees[k].SetVariableValue("newTargetName", GetTargetName(child.GetComponent<Unit>() , enemyCount, playerid, enemyid, group, provoke));

                SetDefend(group, agentTrees[k], defendObject, playerid, king, child.GetComponent<Unit>() );
                
                if (!child.GetComponent<Unit>().isLeader )
                {
                    agentTrees[k].SetVariableValue("newLeader", leaders[playerid][leaderUnitTypeID]);
                } else {
                    agentTrees[k].SetVariableValue("newLeader", null);
                }

                List<BehaviorTree> groupBehaviorTrees;
                Dictionary<int ,  List<BehaviorTree>> leaderBehaviorTrees;
                if (!behaviorTreeGroups[playerid].TryGetValue(leaderUnitTypeID, out leaderBehaviorTrees)) {
                    leaderBehaviorTrees = new Dictionary<int, List<BehaviorTree>>();
                    behaviorTreeGroups[playerid].Add(leaderUnitTypeID, leaderBehaviorTrees);
                }
                if (!behaviorTreeGroups[playerid][leaderUnitTypeID].TryGetValue(group, out groupBehaviorTrees))
                {
                    groupBehaviorTrees = new List<BehaviorTree>();
                    leaderBehaviorTrees.Add(group, groupBehaviorTrees);
                }
                groupBehaviorTrees.Add(agentTrees[k]);
      
            }
        }
        //printTB();

        //if (playerid == 0 || ((RTSNetworkManager)NetworkManager.singleton).Players.Count > 1)
        //{
            //Debug.Log($"LeaderUpdated?.Invoke playerid {playerid} count {((RTSNetworkManager)NetworkManager.singleton).Players.Count} ");
            //LeaderUpdated?.Invoke(leaders[playerid]);
        //}

        AutoRun(playerid);
        //stopwatch.Stop();
        //if (playerid == 0)
        //    Debug.Log($"TacticalFormation ============================ End {stopwatch.ElapsedMilliseconds} milli seconrds. !!!! playerid {playerid} , Leader Count {leaders[playerid].Count} ");
    }
    private string GetTargetName(Unit unit, int enemyCount, int playerid, int enemyid, int group, bool provoke)
    {
        string target = "";
        if (!ISGATEOPENED[playerid]) {
            if (unit.unitType != UnitMeta.UnitType.KING && unit.unitType != UnitMeta.UnitType.HERO)
            {
                target = "Door";
                return target;
            }
        }
        if (TaticalAttackCurrent[playerid] == TaticalAttack.SPINATTACK || TaticalAttackCurrent[playerid] == TaticalAttack.ARROWRAIN || TaticalAttackCurrent[playerid] == TaticalAttack.CAVALRYCHARGES)
        {
            if (provoke)
                target ="Provoke" + enemyid;
            else
                target = enemyCount > 0 ? "Player" + enemyid : "King" + enemyid;
            return target;
        }
        if (group == (int)BehaviorSelectionType.Hold || group == (int)BehaviorSelectionType.Defend) {
            target = enemyCount > 0 ?  "Player" + enemyid : "King" + enemyid;
            return target;
        }
        if (unit.tag.Contains("Sneaky")) {
            target = "King" + enemyid;
            return target;
        }
        if (provoke) {
            target = "Provoke" + enemyid;
            unit.GetUnitMovement().provoke(true);
            return target;
        }
        unit.GetUnitMovement().provoke(false);
        if (unit.unitType == UnitMeta.UnitType.ARCHER || unit.unitType == UnitMeta.UnitType.FOOTMAN)
            target = enemyCount == 0 ? "King" + enemyid : "Player" + enemyid;
        else
            target = "King" + enemyid;

         
        return target;
    }
    private void SetDefend(int group, BehaviorTree agentTree, GameObject defendObject, int playerid, GameObject king,  Unit unit)
    {
        if (group == (int)BehaviorSelectionType.Hold || group == (int)BehaviorSelectionType.Defend)
        {
            float radius = newRadius;
            float defendRadius = newDefendRadius;
            float chaseDistance = 10f;
            UnitMeta.DefendRadius.TryGetValue(unit.unitType, out defendRadius);
            if (TaticalAttackCurrent[playerid] == TaticalAttack.SPINATTACK) {
                unit.GetComponent<UnitPowerUp>().SetSpeed(2f, false);
                if (unit.unitType != UnitMeta.UnitType.KING)
                {
                    defendObject = king;
                    unit.GetComponent<UnitPowerUp>().SetSpeed(7f,false);
                    unit.GetComponent<UnitPowerUp>().SetSkill(UnitMeta.UnitSkill.SHIELD, 1, 1, 1, 1);
                }
                radius = 5f;
                defendRadius = 2.5f;
                chaseDistance = 2.5f;
            }
            if (TaticalAttackCurrent[playerid] == TaticalAttack.ARROWRAIN)
            {
                unit.GetComponent<UnitPowerUp>().SetSkill(UnitMeta.UnitSkill.ARROWRAIN, 1, 1, 1, 1);
            }
            if (TaticalAttackCurrent[playerid] == TaticalAttack.ABSOLUTEDEFENSE)
            {
                if (unit.tag.Contains(playerid.ToString()) && unit.unitType == UnitMeta.UnitType.TANK)
                {
                    defendObject = gameBoardHandlerPrefab.GetSpawnPointObjectByIndex(UnitMeta.UnitType.FOOTMAN, playerid, unit.GetSpawnPointIndex() );
                    radius = .1f;
                    defendRadius = .5f;
                    chaseDistance = .5f;
                }
            }
            agentTree.SetVariableValue("newDefendObject", defendObject);
            agentTree.SetVariableValue("newRadius", radius);
            agentTree.SetVariableValue("newDefendRadius", defendRadius);
            agentTree.SetVariableValue("ChaseDistance", chaseDistance);
            if (defendObject.TryGetComponent<DefendCircle>(out DefendCircle circle))
            {
                //The radius around the defend object to defend
                //circle.DoRenderer(newDefendRadius);
                //The maximum distance that the agents can defend from the defend object
            }
        }
    }
    public void HandleLeaderSelected(int leaderId)
    {
        selectedLeaderId = leaderId;
    }
    public void TryTB(int type )
    {
        TryTB(type, PLAYERID);
    }
    public void TryTB(int type, UnitMeta.UnitType unitType, int playerid)
    {
        int leaderid = 0;
        foreach (var leader in leaders[playerid])
        {
            if (leader.Value.GetComponent<Unit>().unitType == unitType)
            {
                leaderid = leader.Key;
                break;
            }
        }
        TryTB(type, playerid, leaderid);
    }
    public void TryTB(int type, UnitMeta.UnitType unitType)
    {
        int leaderid = 0;
        foreach (var leader in leaders[PLAYERID] )
        {
            if(leader.Value.GetComponent<Unit>().unitType == unitType)
            {
                leaderid = leader.Key;
                break;
            }
        }
        TryTB(type, PLAYERID, leaderid);
    }
    public void TryTB(int type, int playerid)
    {
        TryTB(type, playerid, GetLeaderID(playerid));
    }
    public void TryTB(int type, int playerid, int leaderid)
    {
        LeaderTacticalType(playerid, leaderid, (BehaviorSelectionType)type);
        SelectionChanged(playerid, leaderid);
    }
    public void TryReinforce(Unit unit)
    {
        //Make sure if enemy spawned, need to update TB formation for new target
        //Debug.Log($"Unit spawned {unit.name} {unit.tag}");
        StartCoroutine(TacticalFormation(PLAYERID, ENEMYID, unit.gameObject));
    }

    private void SelectionChanged(int playerID, int leaderid)
    {
        StopCoroutine(EnableBehavior(playerID, leaderid));
        StartCoroutine(DisableBehavior(playerID, leaderid));
        StartCoroutine(EnableBehavior(playerID, leaderid));
    }
    public void StopTacticalBehavior(int playerID, UnitMeta.UnitType unitType)
    {
       //Debug.Log($"StopTacticalBehavior {unitType} ");
        int leaderid = 0;
            foreach (var leader in leaders[PLAYERID])
            {
                if (leader.Value.GetComponent<Unit>().unitType == unitType)
                {
                    leaderid = leader.Key;
                    break;
                }
            }
            //Debug.Log($"StopTacticalBehavior {unitType} leaderid {leaderid}");
            StopCoroutine(EnableBehavior(playerID, leaderid));
            StartCoroutine(DisableBehavior(playerID, leaderid));
    }
    public void StopAllTacticalBehavior(int playerId)
    {
        foreach (var leader in leaders[playerId])
        {
            int leaderid = leader.Key;
            //Debug.Log($"StopTacticalBehavior {leader.Value.name} leaderid {leaderid}");
            StopCoroutine(EnableBehavior(playerId, leaderid));
            StartCoroutine(DisableBehavior(playerId, leaderid));
        }
        //printTB();
    }
    private IEnumerator EnableBehavior(int playerid, int leaderid)
    {
        if (!behaviorTreeGroups[playerid].ContainsKey(leaderid)) { yield break; }
        int localSelectionType = (int) GetLeaderBehaviorSelectionType(playerid, leaderid, true);

        if (!behaviorTreeGroups[playerid][leaderid].ContainsKey(localSelectionType)) { yield break; }
        int agentCount = behaviorTreeGroups[playerid][leaderid][localSelectionType].Count;
        for (int i = 0; i < agentCount; ++i)
        {
            if (behaviorTreeGroups[playerid][leaderid][localSelectionType][i] != null)
            {
                behaviorTreeGroups[playerid][leaderid][localSelectionType][i].EnableBehavior();
            }
        }
    }
    public IEnumerator DisableBehavior(int playerid, int leaderid)
    {
        if (!behaviorTreeGroups[playerid].ContainsKey(leaderid)) { yield break; }
        int localSelectionType = (int)GetLeaderBehaviorSelectionType(playerid, leaderid, false);
        if (!behaviorTreeGroups[playerid][leaderid].ContainsKey(localSelectionType)) { yield break; }
        int agentCount = behaviorTreeGroups[playerid][leaderid][localSelectionType].Count;
        //Debug.Log("DisableBehavior");
        for (int i = 0; i < agentCount; ++i)
        {
            behaviorTreeGroups[playerid][leaderid][localSelectionType][i].DisableBehavior();
        }
    }
    public string GetTacticalStatus()
    {
        List<GameObject> troops = GetAllTroops(PLAYERID);
        troops.AddRange(GetAllTroops(ENEMYID));
        var sb = new System.Text.StringBuilder();
        foreach (GameObject army in troops) {
            sb.Append( String.Format("{0} \t {1} \n", army.name.PadRight(15), army.GetComponent<Unit>().GetTaskStatus().text )) ;
        }
        //return sb.ToString();
        return "";
    }
    public List<GameObject> GetAllTroops(int id)
    {
        List<GameObject> troops;
        GameObject[] armies = GameObject.FindGameObjectsWithTag("Player" + id);
        troops = armies.ToList();
        armies = GameObject.FindGameObjectsWithTag("King" + id);
        if (armies.Length > 0)
            troops.AddRange(armies);
        armies = GameObject.FindGameObjectsWithTag("Provoke" + id);
        if (armies.Length > 0)
            troops.AddRange(armies);
         
        return troops;
    }
    public BehaviorSelectionType GetBehaviorSelectionType(int playerid)
    {
        return GetLeaderBehaviorSelectionType( playerid , GetLeaderID(playerid) , true ) ;
    }
    public BehaviorSelectionType GetLeaderBehaviorSelectionType(int playerid, int leaderid, bool isCurrent)
    {
        if (leaderTacticalType[playerid].ContainsKey(leaderid))
            return leaderTacticalType[playerid][leaderid][isCurrent ? 0 : 1];
        else
        {
            if(!unitTactical[playerid].ContainsKey((UnitMeta.UnitType)leaderid) )
                Debug.Log($"Exception Default GetLeaderBehaviorSelectionType playerid {playerid} leaderid {leaderid} ");
            return unitTactical[playerid][(UnitMeta.UnitType)leaderid];
        }
    }
    public void LeaderTacticalType(int playerid, int leaderid, BehaviorSelectionType type)
    {
        List<BehaviorSelectionType> behaviorSelectionTypes;
        if (!leaderTacticalType[playerid].TryGetValue(leaderid, out behaviorSelectionTypes))
        {
            behaviorSelectionTypes = new  List<BehaviorSelectionType>();
            behaviorSelectionTypes.Add(type);
            behaviorSelectionTypes.Add(type);
            leaderTacticalType[playerid].Add(leaderid, behaviorSelectionTypes);
        }
        behaviorSelectionTypes[1] = behaviorSelectionTypes[0];
        behaviorSelectionTypes[0] = type;
    }
   
    void printTB() {
        StringBuilder sb = new StringBuilder("Pretty Tactial Behavioru Leader Tree \n\n");

        foreach (var ids in behaviorTreeGroups)
        {
            sb.Append($"Player Enemy ID {ids.Key} \n");
            foreach (var _leaders in ids.Value)
            {
                if (leaders[ids.Key].ContainsKey(_leaders.Key))
                {
                    if (!leaderTacticalType[ids.Key].ContainsKey(_leaders.Key))
                        sb.Append($"\t leader id { leaders[ids.Key][_leaders.Key].name } default { unitTactical[ids.Key][(UnitMeta.UnitType)_leaders.Key]} (previous default : { unitTactical[ids.Key][(UnitMeta.UnitType)_leaders.Key]} )   \n");
                    else
                        sb.Append($"\t leader id {leaders[ids.Key][_leaders.Key].name} {leaderTacticalType[ids.Key][_leaders.Key][0]} (previous : {leaderTacticalType[ids.Key][_leaders.Key][1]} )   \n");
                }
                /*
                foreach (var groups in _leaders.Value)
                {
                    sb.Append($" \t\t group {groups.Key} \n");
                    foreach (var agent in groups.Value)
                    {
                        sb.Append($"\t\t\t agent {agent}\n");
                    }
                }
                */
            }
        }
        Debug.Log(sb.ToString());
    }
    public void HandleGameOver(string winner)
    {
        //Debug.Log($"Tactical Behavior ==> HandleGameOver");
        foreach (var playerid in leaders.Keys.ToList())
        {
            foreach (var leaderid in leaders[PLAYERID].Keys.ToList())
            {
                StopCoroutine(EnableBehavior(playerid, leaderid));
                StartCoroutine(DisableBehavior(playerid, leaderid));
            }
        }
    }
    int GetLeaderID(int playerID)
    {
        return (playerID == 1 && ((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1) ? selectedEnemyLeaderId : selectedLeaderId;
    }
    void AutoRun(int playerid)
    {
        foreach (var leaderid in leaders[playerid].Keys.ToList())
        {
            //Debug.Log($"Auto Run leader {playerid} {leaderid}");
            SelectionChanged(playerid , leaderid);
        }
        //AttackOnlyUnit();
    }
    public void taticalAttack(TaticalAttack type, int playerid )
    {
        if(usedTaticalAttack == true) { return; }
        StartCoroutine( HandleTaticalAttack(type, playerid ));
    }
    IEnumerator HandleTaticalAttack(TaticalAttack type, int playerid )
    {
        usedTaticalAttack = true;
        Vector3 kingPoint = GameObject.FindGameObjectWithTag("King" + playerid).transform.position;
        Vector3 spawnPoint = Vector3.zero;
        int unitspawn = 1;
        UnitFactory localFactory=null;
        CardStats cardStats;
        float offset = 1f;
        int posxflip = -1;
        int poszflip = 1;
        if (((RTSNetworkManager)NetworkManager.singleton).Players.Count > 1) {
            poszflip = 1;
        } else {
            poszflip = (playerid == PLAYERID ? 1 : -1);
        }
        Dictionary<string, CardStats> userCardStatsDict = GameObject.FindGameObjectWithTag("DealManager").GetComponent<CardDealer>().userCardStatsDict;
        foreach (GameObject factroy in GameObject.FindGameObjectsWithTag("UnitFactory"))
        {
            if (factroy.GetComponent<UnitFactory>().hasAuthority)
            {
                localFactory = factroy.GetComponent<UnitFactory>();
            }
        }
        switch (type) {
            case TaticalAttack.SPINATTACK:
                TaticalAttackCurrent[playerid] = TaticalAttack.SPINATTACK;
                cardStats = userCardStatsDict[UnitMeta.UnitRaceTypeKey[StaticClass.playerRace][UnitMeta.UnitType.FOOTMAN].ToString()];
                while (unitspawn <= 3) {
                    yield return new WaitForSeconds(0.5f);
                    localFactory.CmdSpawnUnit(StaticClass.playerRace, UnitMeta.UnitType.FOOTMAN, 3, playerid, cardStats.cardLevel, cardStats.health, cardStats.attack, cardStats.repeatAttackDelay, cardStats.speed, cardStats.defense, cardStats.special, cardStats.specialkey, cardStats.passivekey, player.GetTeamColor());
                    unitspawn++;
                }
                TryTB((int)BehaviorSelectionType.Defend, UnitMeta.UnitType.FOOTMAN, playerid);
                //yield return new WaitForSeconds(4f);
                TryTB((int)BehaviorSelectionType.Attack, UnitMeta.UnitType.HERO, playerid);
                TryTB((int)BehaviorSelectionType.Attack, UnitMeta.UnitType.KING, playerid);
                
                break;
            case TaticalAttack.CAVALRYCHARGES:
                TaticalAttackCurrent[playerid] = TaticalAttack.CAVALRYCHARGES;
                cardStats = userCardStatsDict[UnitMeta.UnitRaceTypeKey[StaticClass.playerRace][UnitMeta.UnitType.CAVALRY].ToString()];
                TryTB((int)BehaviorSelectionType.Hold, UnitMeta.UnitType.CAVALRY, playerid);
                unitTactical[playerid][UnitMeta.UnitType.CAVALRY] = BehaviorSelectionType.Hold;
                while (unitspawn <= 6)
                {
                    yield return new WaitForSeconds(0.5f);
                    posxflip *= -1;
                    spawnPoint = new Vector3(kingPoint.x + (offset * posxflip), kingPoint.y , kingPoint.z + (10 * poszflip));
                    localFactory.CmdDropUnit(playerid, spawnPoint, StaticClass.playerRace, UnitMeta.UnitType.CAVALRY , UnitMeta.UnitType.CAVALRY.ToString(), 1, cardStats.cardLevel, cardStats.health, cardStats.attack, cardStats.repeatAttackDelay, cardStats.speed, cardStats.defense, cardStats.special, cardStats.specialkey, cardStats.passivekey, 1, player.GetTeamColor(), Quaternion.identity);
                    offset += 2;
                    unitspawn++;
                }
                TryTB((int)BehaviorSelectionType.Charge, UnitMeta.UnitType.CAVALRY, playerid);
                //yield return new WaitForSeconds(4f);
                TryTB((int)BehaviorSelectionType.Attack, UnitMeta.UnitType.HERO, playerid);
                TryTB((int)BehaviorSelectionType.Attack, UnitMeta.UnitType.KING, playerid);

                break;
            case TaticalAttack.ARROWRAIN:
                TaticalAttackCurrent[playerid] = TaticalAttack.ARROWRAIN;
                cardStats = userCardStatsDict[UnitMeta.UnitRaceTypeKey[StaticClass.playerRace][UnitMeta.UnitType.ARCHER].ToString()];
                TryTB((int)BehaviorSelectionType.Hold, UnitMeta.UnitType.ARCHER, playerid);
                unitTactical[playerid][UnitMeta.UnitType.ARCHER] = BehaviorSelectionType.Hold;
                while (unitspawn <= 12)
                {
                    yield return new WaitForSeconds(.5f);
                    posxflip *= -1;
                    spawnPoint = new Vector3(kingPoint.x + (offset * posxflip), kingPoint.y, kingPoint.z + (10 * poszflip));
                    localFactory.CmdDropUnit(playerid, spawnPoint, StaticClass.playerRace, UnitMeta.UnitType.ARCHER, UnitMeta.UnitType.ARCHER.ToString(), 1, cardStats.cardLevel, cardStats.health, cardStats.attack, cardStats.repeatAttackDelay, cardStats.speed, cardStats.defense, cardStats.special, cardStats.specialkey, cardStats.passivekey, 1, player.GetTeamColor(), Quaternion.identity);
                    offset += 1;
                    unitspawn++;
                }
                TryTB((int)BehaviorSelectionType.Attack , UnitMeta.UnitType.ARCHER, playerid);
                break;
            case TaticalAttack.ABSOLUTEDEFENSE:
                TaticalAttackCurrent[playerid] = TaticalAttack.ABSOLUTEDEFENSE;
                cardStats = userCardStatsDict[UnitMeta.UnitRaceTypeKey[StaticClass.playerRace][UnitMeta.UnitType.TANK].ToString()];
                TryTB((int)BehaviorSelectionType.Defend, UnitMeta.UnitType.TANK, playerid);
                unitTactical[playerid][UnitMeta.UnitType.TANK] = BehaviorSelectionType.Defend;
                while (unitspawn <= 5)
                {
                    yield return new WaitForSeconds(.5f);
                    localFactory.CmdSpawnUnitPosition(StaticClass.playerRace, UnitMeta.UnitType.TANK, 2, playerid, cardStats.cardLevel, cardStats.health, cardStats.attack, cardStats.repeatAttackDelay, cardStats.speed, cardStats.defense, cardStats.special, cardStats.specialkey, cardStats.passivekey, player.GetTeamColor(), UnitMeta.UnitType.FOOTMAN);
                    unitspawn++;
                }
                TryTB((int)BehaviorSelectionType.Defend, UnitMeta.UnitType.TANK, playerid);
                break;
            default:
                break;
        }
        yield return null;
    }
    public void AllUnitCommand(int type)
    {
        BehaviorSelectionType behaviorType = BehaviorSelectionType.Attack;
        foreach (var leaderid in leaders[PLAYERID].Keys.ToList())
        {
            if (type == 0)
            {
                behaviorType = ((leaderid) == (int)UnitMeta.UnitType.ARCHER) ? BehaviorSelectionType.Flank : BehaviorSelectionType.Attack;
            }
            else
            {
                behaviorType = BehaviorSelectionType.Defend;
            }
            LeaderTacticalType(PLAYERID, leaderid, behaviorType);
            SelectionChanged(PLAYERID, leaderid);
        }
    }
    
    public void SetKingBoss(int enemyid, GameObject boss)
    {
        KINGBOSS[enemyid] = boss;
    }
    public void SetGateOpen(string playerid)
    {

        ISGATEOPENED[Int32.Parse(playerid)] = true;
    }
    #endregion
}
