
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
    public static event Action<Dictionary<int, GameObject>> LeaderUpdated;
    private eleixier Eleixier;
    private RTSPlayer player;
    private int PLAYERID = 0;
    private int ENEMYID = 0;
    
    private Dictionary<int, List<BehaviorTree>> leaderPlayerBehaviorTreeGroup = new Dictionary<int, List<BehaviorTree>>();
    private Dictionary<int, List<BehaviorTree>> leaderEnemyBehaviorTreeGroup = new Dictionary<int, List<BehaviorTree>>();
    private Dictionary<int, Dictionary<int, List<BehaviorTree>>> playerBehaviorTreeGroup = new Dictionary<int, Dictionary<int, List<BehaviorTree>>>();
    private Dictionary<int, Dictionary<int, List<BehaviorTree>>> enemyBehaviorTreeGroup = new Dictionary<int, Dictionary<int, List<BehaviorTree>>>();

    private Dictionary<int, Dictionary<int, Dictionary<int, List<BehaviorTree>>>> behaviorTreeGroups = new Dictionary<int, Dictionary < int, Dictionary<int, List<BehaviorTree>>>>();

    public enum BehaviorSelectionType { Attack, Charge, MarchingFire, Flank, Ambush, ShootAndScoot, Leapfrog, Surround, Defend, Hold, Retreat, Reinforcements, Last }

    private Dictionary<int, Dictionary<int, GameObject>> leaders = new Dictionary<int, Dictionary<int, GameObject>>();
    private Dictionary<int, Dictionary<int,  List<BehaviorSelectionType>>> leaderTacticalType = new Dictionary<int, Dictionary<int, List<BehaviorSelectionType>>>();
    private Color teamColor;
    private Color teamEnemyColor;
    private int selectedLeaderId = 0;
    private int selectedEnemyLeaderId = 0;
    private GameBoardHandler gameBoardHandlerPrefab = null;
    private Dictionary<int, GameObject> KINGBOSS = new Dictionary<int, GameObject>();
    private float radius = 0.1f;
    private float newDefendRadius = 7f;
    private static bool IS_STUNNED = false;
    #region Client

    public void Start()
    {
        Eleixier = FindObjectOfType<eleixier>();
        //if (NetworkClient.connection.identity == null) { return; }

        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        PLAYERID = player.GetPlayerID();
        ENEMYID = player.GetEnemyID();

        StartCoroutine(AssignTag());

        behaviorTreeGroups.Add(PLAYERID, playerBehaviorTreeGroup);
        behaviorTreeGroups.Add(ENEMYID, enemyBehaviorTreeGroup);
        leaderTacticalType.Add(PLAYERID, new Dictionary<int, List<BehaviorSelectionType>>() );
        leaderTacticalType.Add(ENEMYID, new Dictionary<int, List<BehaviorSelectionType>>() );
        leaders.Add(PLAYERID, new Dictionary<int, GameObject>());
        leaders.Add(ENEMYID, new Dictionary<int, GameObject>());

        teamColor = player.GetTeamColor();
        teamEnemyColor = player.GetTeamEnemyColor();
       
        //Make sure if enemy spawned, need to update TB formation for new target
        Unit.ClientOnUnitSpawned += TryReinforce;
        Unit.ClientOnUnitDespawned += TryReinforce;
        //LeaderScrollList.LeaderSelected += HandleLeaderSelected;
        GameOverHandler.ClientOnGameOver += HandleGameOver;
    }
    public void OnDestroy()
    {
        Unit.ClientOnUnitSpawned -= TryReinforce;
        Unit.ClientOnUnitDespawned -= TryReinforce;
        GameOverHandler.ClientOnGameOver -= HandleGameOver;
        //LeaderScrollList.LeaderSelected -= HandleLeaderSelected;
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
            yield return new WaitForSeconds(1f);

            GameObject[] armies = GameObject.FindGameObjectsWithTag("Unit");
            //Debug.Log($"army{armies.Length}");
            foreach (GameObject army in armies)
            {
                // Debug.Log($"army-->{army}");
                // Debug.Log($"Assign Tag for Client name {army.name} ");
                if (army.TryGetComponent<Unit>(out Unit unit))
                {
                    if (unit.hasAuthority)
                    {
                        //Debug.Log("unit.hasAuthority");
                        unit.GetComponent<HealthDisplay>().SetHealthBarColor(teamColor);
                        unit.GetComponent<UnitBody>().SetRenderMaterial(unit.transform.gameObject, player.GetPlayerID(), 1);
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
                        unit.GetComponent<HealthDisplay>().SetHealthBarColor(teamEnemyColor);
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
            yield return new WaitForSeconds(2f);
            //Debug.Log($"player0: {GameObject.FindGameObjectsWithTag("Player0").Length} / Player2 : {GameObject.FindGameObjectsWithTag("Player1").Length}");
            if ( GameObject.FindGameObjectsWithTag("Player0").Length > 0 && GameObject.FindGameObjectsWithTag("King0").Length > 0  && GameObject.FindGameObjectsWithTag("Player1").Length > 0 && GameObject.FindGameObjectsWithTag("King1").Length > 0)
            {
                ISTAGGED = true;
                yield return new WaitForSeconds(1f);
                yield return TacticalFormation(PLAYERID, ENEMYID);
            }

        }
        //Debug.Log("AssignTag ============================ END ");

    }
    public IEnumerator TacticalFormation(int playerid, int enemyid)
    {
        yield return new WaitForSeconds(0.1f);
        if (gameBoardHandlerPrefab == null) { yield break; }
        if (IS_STUNNED) { yield break;}
        //var stopwatch = new Stopwatch();
        //stopwatch.Start();

        GameObject[] units = GameObject.FindGameObjectsWithTag("Player" + playerid);
        GameObject king = GameObject.FindGameObjectWithTag("King" + playerid);
        List<GameObject> armies = new List<GameObject>();
        armies = units.ToList();
        if(king!=null)
            armies.Add(king);
        GameObject defendObject;

        //if (playerid == 0)
        //    Debug.Log($"TacticalFormation ============================ Start playerid {playerid} armis size {armies.Length}");

        leaders[playerid].Clear();
        int i = 0;
        radius = 0.1f;
        newDefendRadius = 7f;
        int enemyCount = GameObject.FindGameObjectsWithTag("Player" + enemyid).Length;
        behaviorTreeGroups[playerid].Clear();
        int leaderUnitTypeID = 0;
       
        
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
            i++;
        }
        for (int j = 0; j < PlayerEnemyGroup[playerid].transform.childCount; ++j)
        {
            var child = PlayerEnemyGroup[playerid].transform.GetChild(j);
            leaderUnitTypeID = (int)child.GetComponent<Unit>().unitType;
      
            //if (child.GetComponent<Unit>().unitType == UnitMeta.UnitType.HERO) {
            //    defendObject = KINGBOSS[playerid];
            //    defendRadius = 10;
            //}
            //else {
                //Debug.Log($"Player {playerid} Unit {(UnitMeta.UnitType)leaderUnitTypeID } Spawn Point Index {child.GetComponent<Unit>().GetSpawnPointIndex()} gameBoardHandlerPrefab: {gameBoardHandlerPrefab == null} ");
                defendObject = gameBoardHandlerPrefab.GetSpawnPointObjectByIndex( (UnitMeta.UnitType) leaderUnitTypeID , playerid, child.GetComponent<Unit>().GetSpawnPointIndex());
                radius = 0.1f;
            //}
            var agentTrees = child.GetComponents<BehaviorTree>();
            for (int k = 0; k < agentTrees.Length; ++k)
            {
                var group = agentTrees[k].Group;
                
                if (group == 1 || group ==2 || group == 4 || group == 6 || group == 9 || group == 11 || group == 12) { continue; }
                agentTrees[k].SetVariableValue("newTargetName", "King" + enemyid);
                if (child.GetComponent<Unit>().unitType == UnitMeta.UnitType.ARCHER)
                    agentTrees[k].SetVariableValue("newTargetName", enemyCount == 0 ? "King" + enemyid : "Player" + enemyid);
                else
                    agentTrees[k].SetVariableValue("newTargetName", "King" + enemyid);
                if (group == (int)BehaviorSelectionType.Hold || group == (int)BehaviorSelectionType.Defend)
                {
                    agentTrees[k].SetVariableValue("newDefendObject", defendObject);
                    agentTrees[k].SetVariableValue("newRadius", radius);
                    agentTrees[k].SetVariableValue("newDefendRadius", newDefendRadius);
                    if (UnitMeta.DefendRadius.TryGetValue(child.GetComponent<Unit>().unitType, out newDefendRadius))
                        agentTrees[k].SetVariableValue("newDefendRadius", newDefendRadius);

                    agentTrees[k].SetVariableValue("ChaseDistance", 10f);
                    agentTrees[k].SetVariableValue("newTargetName", "Player" + enemyid);
                    if (defendObject.TryGetComponent<DefendCircle>(out DefendCircle circle))
                    {
                        //The radius around the defend object to defend
                        circle.DoRenderer(newDefendRadius);
                        //The maximum distance that the agents can defend from the defend object
                    }
                }
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
    public void HandleLeaderSelected(int leaderId)
    {
        selectedLeaderId = leaderId;
    }
    public void HandleStun(bool stun)
    {
        TacticalBehavior.IS_STUNNED = stun;
    }
    public void TryTB(int type )
    {
        TryTB(type, PLAYERID);
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
        //Eleixier.speedUpEleixier(GetBehaviorSelectionType(playerid));
        SelectionChanged(playerid, leaderid);
    }
    public void TryReinforce(Unit unit)
    {
        //Make sure if enemy spawned, need to update TB formation for new target
        StartCoroutine(TacticalFormation(PLAYERID, ENEMYID));
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
        foreach (var leader in leaders[PLAYERID])
        {
            int leaderid = leader.Key;
            Debug.Log($"StopTacticalBehavior {leader.Value.name} leaderid {leaderid}");
            StopCoroutine(EnableBehavior(playerId, leaderid));
            StartCoroutine(DisableBehavior(playerId, leaderid));
        }
    }
    private IEnumerator EnableBehavior(int playerid, int leaderid)
    {
        if (!behaviorTreeGroups[playerid].ContainsKey(leaderid)) { yield break; }
        int localSelectionType = (int) GetLeaderBehaviorSelectionType(playerid, leaderid, true);
        //if(localSelectionType == (int) BehaviorSelectionType.Defend) EnableDefendRadius(playerid);
        //if ((BehaviorSelectionType)localSelectionType == BehaviorSelectionType.Defend && GameObject.FindGameObjectsWithTag("Player" + (playerid == 0 ? 1 : 0) ).Length == 0) { yield break;  }
        
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
        int agentCount = behaviorTreeGroups[playerid][leaderid][localSelectionType].Count;
        //Debug.Log("DisableBehavior");
        for (int i = 0; i < agentCount; ++i)
        {
            behaviorTreeGroups[playerid][leaderid][localSelectionType][i].DisableBehavior();
           
        }
    }
    public string GetTacticalStatus()
    {
        List<GameObject> troops;
        var sb = new System.Text.StringBuilder();
        GameObject[] armies = GameObject.FindGameObjectsWithTag("Player" + PLAYERID);
        troops = armies.ToList();
        armies = GameObject.FindGameObjectsWithTag("King" + PLAYERID);
        if(armies.Length > 0)
        troops.AddRange(armies);
        foreach (GameObject army in troops) {
            sb.Append( String.Format("{0} \t {1} \n", army.name.PadRight(15), army.GetComponent<Unit>().GetTaskStatus().text )) ;
        }
        return sb.ToString();
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
            if(!UnitMeta.DefaultUnitTactical.ContainsKey((UnitMeta.UnitType)leaderid) )
                Debug.Log($"Exception Default GetLeaderBehaviorSelectionType playerid {playerid} leaderid {leaderid} ");
            return UnitMeta.DefaultUnitTactical[(UnitMeta.UnitType)leaderid];
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
            foreach (var leaders in ids.Value)
            {
                if (!leaderTacticalType[ids.Key].ContainsKey(leaders.Key))
                    sb.Append($"\t leader id {leaders.Key} default { UnitMeta.DefaultUnitTactical[ (UnitMeta.UnitType) leaders.Key  ]} (previous default : { UnitMeta.DefaultUnitTactical[(UnitMeta.UnitType)leaders.Key]} )   \n");
                else
                    sb.Append($"\t leader id {leaders.Key} {leaderTacticalType[ids.Key][leaders.Key][0]} (previous : {leaderTacticalType[ids.Key][leaders.Key][1]} )   \n");
                foreach (var groups in leaders.Value)
                {
                    sb.Append($" \t\t group {groups.Key} \n");
                    foreach (var agent in groups.Value)
                    {
                        sb.Append($"\t\t\t agent {agent}\n");
                    }
                }
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
    void AttackOnlyUnit(int playerid, int enemyID)
    {
        int leaderUnitTypeID = 99;
        for (int j = 0; j < PlayerEnemyGroup[playerid].transform.childCount; ++j)
        {
            var child = PlayerEnemyGroup[playerid].transform.GetChild(j);
            if (child.GetComponent<Unit>().unitType == UnitMeta.UnitType.FOOTMAN) // ATTACK ONLY , cannot go back base
            {
                var agentTrees = child.GetComponents<BehaviorTree>();
                for (int k = 0; k < agentTrees.Length; ++k)
                {
                    var group = agentTrees[k].Group;
                    if (group != 0 || group != 3 ) { continue; }

                    agentTrees[k].SetVariableValue("newTargetName", "Player" + enemyID);
                    if(j == 0)
                        leaders[playerid].Add(leaderUnitTypeID, child.gameObject);
                    agentTrees[k].SetVariableValue("newLeader", (j == 0) ? child.gameObject : null );

                    List<BehaviorTree> groupBehaviorTrees;
                    Dictionary<int, List<BehaviorTree>> leaderBehaviorTrees;
                    if (!behaviorTreeGroups[playerid].TryGetValue(leaderUnitTypeID, out leaderBehaviorTrees))
                    {
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
        }
        LeaderTacticalType(playerid, leaderUnitTypeID, BehaviorSelectionType.Attack );
    }
    private void EnableDefendRadius(int playerid)
    {
        Vector3 defendRadius = new Vector3(0.03f, 0.03f, 0.03f);
        Vector3 radius = new Vector3(0.1f, 0.1f, 0.1f);
        for (int j = 0; j < PlayerEnemyGroup[playerid].transform.childCount; ++j)
        {
            var child = PlayerEnemyGroup[playerid].transform.GetChild(j);
            if (child.GetComponent<Unit>().unitType == UnitMeta.UnitType.HERO) // ATTACK ONLY , cannot go back base
            {
                child.GetComponent<Unit>().GetUnitMovement().circleMarker.SetActive(true);
                child.GetComponent<Unit>().GetUnitMovement().circleMarker.transform.localScale = defendRadius;
                if (KINGBOSS[playerid].TryGetComponent<Unit>(out Unit kingboss))
                {
                    kingboss.GetUnitMovement().circleMarker.SetActive(true);
                    kingboss.GetUnitMovement().circleMarker.transform.localScale = radius;
                }
            }
        }
    }
    public void SetKingBoss(int enemyid, GameObject boss)
    {
        KINGBOSS[enemyid] = boss;
    }
    #endregion
}
