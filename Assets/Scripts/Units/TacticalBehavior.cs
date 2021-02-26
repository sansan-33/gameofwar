
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BehaviorDesigner.Runtime;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class TacticalBehavior : MonoBehaviour
{
    [SerializeField] List<GameObject> PlayerEnemyGroup = new List<GameObject>();
    public static event Action<Dictionary<int, GameObject>> LeaderUpdated;
    private eleixier Eleixier;
    private RTSPlayer player;
    private int PLAYERID = 0;
    private int ENEMYID = 0;
    private string ENEMYTAG = "";
    private string PLAYERTAG = "";

    private Dictionary<int, List<BehaviorTree>> leaderPlayerBehaviorTreeGroup = new Dictionary<int, List<BehaviorTree>>();
    private Dictionary<int, List<BehaviorTree>> leaderEnemyBehaviorTreeGroup = new Dictionary<int, List<BehaviorTree>>();
    private Dictionary<int, Dictionary<int, List<BehaviorTree>>> playerBehaviorTreeGroup = new Dictionary<int, Dictionary<int, List<BehaviorTree>>>();
    private Dictionary<int, Dictionary<int, List<BehaviorTree>>> enemyBehaviorTreeGroup = new Dictionary<int, Dictionary<int, List<BehaviorTree>>>();

    private Dictionary<int, Dictionary<int, Dictionary<int, List<BehaviorTree>>>> behaviorTreeGroups = new Dictionary<int, Dictionary < int, Dictionary<int, List<BehaviorTree>>>>();

    private GameObject defendObject = null;
    public enum BehaviorSelectionType { Attack, Charge, MarchingFire, Flank, Ambush, ShootAndScoot, Leapfrog, Surround, Defend, Hold, Retreat, Reinforcements, Last }

    private Dictionary<int, GameObject> leaders = new Dictionary<int, GameObject>();
    private Dictionary<int, Dictionary< int,  List<BehaviorSelectionType>>> leaderTacticalType = new Dictionary<int, Dictionary<int, List<BehaviorSelectionType>>>();
    private Color teamColor;
    private Color teamEnemyColor;
    private int selectedLeaderId = 0;
    private int selectedEnemyLeaderId = 0;
    #region Client

    public void Awake()
    {
        Eleixier = FindObjectOfType<eleixier>();
        if (NetworkClient.connection.identity == null) { return; }

        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        PLAYERID = player.GetPlayerID();
        ENEMYID = player.GetEnemyID();
        ENEMYTAG = "Player" + ENEMYID;
        PLAYERTAG = "Player" + PLAYERID;
        StartCoroutine(AssignTag());
        StartCoroutine(TacticalFormation(PLAYERID, ENEMYID));
        behaviorTreeGroups.Add(PLAYERID, playerBehaviorTreeGroup);
        behaviorTreeGroups.Add(ENEMYID, enemyBehaviorTreeGroup);
        leaderTacticalType.Add(PLAYERID, new Dictionary<int, List<BehaviorSelectionType>>() );
        leaderTacticalType.Add(ENEMYID, new Dictionary<int, List<BehaviorSelectionType>>() );

        teamColor = player.GetTeamColor();
        teamEnemyColor = player.GetTeamEnemyColor();

        Unit.ServerOnUnitDespawned += TryReinforcePlayer;
        Unit.ServerOnUnitSpawned += TryReinforcePlayer;
        LeaderScrollList.LeaderSelected += HandleLeaderSelected;
    }
    public void OnDestroy()
    {
        Unit.ServerOnUnitDespawned -= TryReinforcePlayer;
        Unit.ServerOnUnitSpawned -= TryReinforcePlayer;
        LeaderScrollList.LeaderSelected -= HandleLeaderSelected;
    }
    public IEnumerator AssignTag()
    {
        yield return new WaitForSeconds(1f);
        GameObject[] playerBases = GameObject.FindGameObjectsWithTag("PlayerBase");
        foreach (GameObject playerBase in playerBases)
        {
            if (playerBase.TryGetComponent<UnitBase>(out UnitBase unit))
            {
                if (unit.hasAuthority)
                {
                    playerBase.tag = "PlayerBase" + PLAYERID;
                }
                else
                {
                    //Only Assing Enemy Base Tag if mulitplayer
                    if(((RTSNetworkManager)NetworkManager.singleton).Players.Count > 1)
                        playerBase.tag = "PlayerBase" + ENEMYID;
                }
            }
        }

        yield return new WaitForSeconds(0.5f);

        GameObject[] armies = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject army in armies)
        {
            if (army.TryGetComponent<Unit>(out Unit unit))
            {
                if (unit.hasAuthority) {
                    unit.GetComponent<HealthDisplay>().SetHealthBarColor(teamColor);
                    army.tag = PLAYERTAG;
                }
                else {
                    //Only Assing Enemy Base Tag if mulitplayer
                    //if (((RTSNetworkManager)NetworkManager.singleton).Players.Count > 1)
                    unit.GetComponent<HealthDisplay>().SetHealthBarColor(teamEnemyColor);
                    army.tag = ENEMYTAG;
                }
            }
        }
    }
    public IEnumerator TacticalFormation(int playerid, int enemyid)
    {
        yield return new WaitForSeconds(1f);
        GameObject[] armies = GameObject.FindGameObjectsWithTag("Player" + playerid);
        leaders.Clear();
        int i = 0;
        behaviorTreeGroups[playerid].Clear();
        int leaderUnitTypeID = 0;
        int randBase = 0;
        GameObject king = null;
        foreach (GameObject child in armies)
        {
            leaderUnitTypeID = (int)child.GetComponent<Unit>().unitType;
            if (child.name.Contains("]"))
            {
                child.name = child.name.Substring(child.name.IndexOf("]") + 2 );
            }
            if (child.GetComponent<Unit>().unitType == UnitMeta.UnitType.KING){
                king = child;
            } else if (!leaders.ContainsKey(leaderUnitTypeID))
            {
                leaders.Add(leaderUnitTypeID, child);
                child.name = "LEADER" + leaders.Count;
            }
            child.name = child.name.Length > 6 ? child.name.Substring(0, 6) : child.name;
            child.name = "[" + i + "]\t" + child.name;
            child.transform.parent = PlayerEnemyGroup[playerid].transform;
            i++;
        }
        for (int j = 0; j < PlayerEnemyGroup[playerid].transform.childCount; ++j)
        {
            var child = PlayerEnemyGroup[playerid].transform.GetChild(j);
            leaderUnitTypeID = (int)child.GetComponent<Unit>().unitType;
          
            randBase = randBase == 0 ? 1 : 0;
            //Debug.Log($"randLeader {rr}/{randLeader}/{leaders.Count} randBase {randBase}");

            if (child.GetComponent<Unit>().unitType == UnitMeta.UnitType.HERO ) {
                defendObject = king;
                defendObject.name = "King";
            }
            else {
                defendObject = GameObject.FindGameObjectsWithTag("PlayerBase" + playerid)[randBase];
                defendObject.name = "PlayerBase" + playerid + randBase;
            }
            var agentTrees = child.GetComponents<BehaviorTree>();
            for (int k = 0; k < agentTrees.Length; ++k)
            {
                var group = agentTrees[k].Group;
                if (group == 1 || group ==2 || group == 4 || group == 6 || group == 9 || group == 11 || group == 12) { continue; }
                agentTrees[k].SetVariableValue("newTargetName", "Player" + enemyid);
                if (k == (int)BehaviorSelectionType.Hold || k == (int)BehaviorSelectionType.Defend)
                {
                    agentTrees[k].SetVariableValue("newDefendObject", defendObject);
                }
                if (!child.gameObject.name.ToUpper().Contains("LEADER"))
                {
                    agentTrees[k].SetVariableValue("newLeader", leaders[leaderUnitTypeID]);
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
        
        if (playerid == 0 || ((RTSNetworkManager)NetworkManager.singleton).Players.Count > 1)
            LeaderUpdated?.Invoke(leaders);

        InitSetupSelectedLeaderID(playerid);
        AutoRun(playerid);

    }
    public void HandleLeaderSelected(int leaderId)
    {
        selectedLeaderId = leaderId;
    }
    public void TryTB(int type )
    {
        TryTB(type, PLAYERID);
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
    public void TryReinforcePlayer(Unit unit)
    {
        if (unit.tag == ENEMYTAG) { return; }
        Debug.Log($"Auto Reinforce ..... {unit.name}");
        StartCoroutine(TacticalFormation(PLAYERID, ENEMYID));
    }
    
    private void SelectionChanged(int playerID, int leaderid)
    {
        StopCoroutine(EnableBehavior(playerID, leaderid));
        StartCoroutine(DisableBehavior(playerID, leaderid));
        StartCoroutine(EnableBehavior(playerID, leaderid));
        Eleixier.speedUpEleixier(GetBehaviorSelectionType(playerID));
    }
    private IEnumerator EnableBehavior(int playerid, int leaderid)
    {
        yield return new WaitForSeconds(0.1f);
        int localSelectionType = (int) GetLeaderBehaviorSelectionType(playerid, leaderid, true);
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
        yield return new WaitForSeconds(0.1f);
        int localSelectionType = (int)GetLeaderBehaviorSelectionType(playerid, leaderid, false);
        int agentCount = behaviorTreeGroups[playerid][leaderid][localSelectionType].Count;

        for (int i = 0; i < agentCount; ++i)
        {
            behaviorTreeGroups[playerid][leaderid][localSelectionType][i].DisableBehavior();
        }
    }
    public string GetTacticalStatus()
    {
        if (PLAYERTAG is null || PLAYERTAG == "") { return ""; }
        var sb = new System.Text.StringBuilder();
        GameObject[] armies = GameObject.FindGameObjectsWithTag(PLAYERTAG);

        foreach (GameObject army in armies) {
            
            sb.Append( String.Format("{0} \t\t {1} \n", army.name, army.GetComponent<Unit>().GetTaskStatus().text )) ;
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
            return UnitMeta.DefaultUnitTactical[ (UnitMeta.UnitType) leaderid ];
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
    void InitSetupSelectedLeaderID(int playerID)
    {
        System.Random rand = new System.Random();

        if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)
        {
            if (playerID == 1)
                selectedEnemyLeaderId = behaviorTreeGroups[playerID].ElementAt(rand.Next(0, behaviorTreeGroups[playerID].Count)).Key;
            else
            {
                if (selectedLeaderId == 0)
                {
                    selectedLeaderId = behaviorTreeGroups[playerID].ElementAt(0).Key;
                }
            }
        }
    }
    int GetLeaderID(int playerID)
    {
        return (playerID == 1 && ((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1) ? selectedEnemyLeaderId : selectedLeaderId;
    }
    void AutoRun(int playerid)
    {
        foreach (var leaderid in leaders.Keys.ToList())
        {
            SelectionChanged(playerid , leaderid);
        }
        //AttackOnlyUnit();
    }
    void AttackOnlyUnit(int playerID, int enemyID)
    {
        int leaderUnitTypeID = 99;
        for (int j = 0; j < PlayerEnemyGroup[playerID].transform.childCount; ++j)
        {
            var child = PlayerEnemyGroup[playerID].transform.GetChild(j);
            if (child.GetComponent<Unit>().unitType == UnitMeta.UnitType.SPEARMAN) // ATTACK ONLY , cannot go back base
            {
                var agentTrees = child.GetComponents<BehaviorTree>();
                for (int k = 0; k < agentTrees.Length; ++k)
                {
                    var group = agentTrees[k].Group;
                    if (group != 0 || group != 3 ) { continue; }

                    agentTrees[k].SetVariableValue("newTargetName", "Player" + enemyID);
                    agentTrees[k].SetVariableValue("newLeader", (j == 0) ? child : null );

                    List<BehaviorTree> groupBehaviorTrees;
                    Dictionary<int, List<BehaviorTree>> leaderBehaviorTrees;
                    if (!behaviorTreeGroups[playerID].TryGetValue(leaderUnitTypeID, out leaderBehaviorTrees))
                    {
                        leaderBehaviorTrees = new Dictionary<int, List<BehaviorTree>>();
                        behaviorTreeGroups[playerID].Add(leaderUnitTypeID, leaderBehaviorTrees);
                    }
                    if (!behaviorTreeGroups[playerID][leaderUnitTypeID].TryGetValue(group, out groupBehaviorTrees))
                    {
                        groupBehaviorTrees = new List<BehaviorTree>();
                        leaderBehaviorTrees.Add(group, groupBehaviorTrees);
                    }
                    groupBehaviorTrees.Add(agentTrees[k]);

                }
            }
        }
        LeaderTacticalType(playerID, leaderUnitTypeID, BehaviorSelectionType.Attack );
        SelectionChanged(playerID, leaderUnitTypeID);
    }
    #endregion
}
