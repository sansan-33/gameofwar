
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
    private int playerid = 0;
    private int enemyid = 0;
    private string ENEMYTAG = "";
    private string PLAYERTAG = "";

    private Dictionary<int, List<BehaviorTree>> leaderPlayerBehaviorTreeGroup = new Dictionary<int, List<BehaviorTree>>();
    private Dictionary<int, List<BehaviorTree>> leaderEnemyBehaviorTreeGroup = new Dictionary<int, List<BehaviorTree>>();
    private Dictionary<int, Dictionary<int, List<BehaviorTree>>> playerBehaviorTreeGroup = new Dictionary<int, Dictionary<int, List<BehaviorTree>>>();
    private Dictionary<int, Dictionary<int, List<BehaviorTree>>> enemyBehaviorTreeGroup = new Dictionary<int, Dictionary<int, List<BehaviorTree>>>();

    private Dictionary<int, Dictionary<int, Dictionary<int, List<BehaviorTree>>>> behaviorTreeGroups = new Dictionary<int, Dictionary < int, Dictionary<int, List<BehaviorTree>>>>();

    private GameObject defendObject = null;
    public enum BehaviorSelectionType { Attack, Charge, MarchingFire, Flank, Ambush, ShootAndScoot, Leapfrog, Surround, Defend, Hold, Retreat, Reinforcements, Last }
    private BehaviorSelectionType selectionType = BehaviorSelectionType.Attack;
    private BehaviorSelectionType prevSelectionType = BehaviorSelectionType.Attack;
    

    private string playerType = "";
    private string enemyType = "";
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
        playerid = player.GetPlayerID();
        enemyid = player.GetEnemyID();
        ENEMYTAG = "Player" + enemyid;
        PLAYERTAG = "Player" + playerid;
        StartCoroutine(AssignTag());
        StartCoroutine(TacticalFormation(playerid, enemyid));
        behaviorTreeGroups.Add(playerid, playerBehaviorTreeGroup);
        behaviorTreeGroups.Add(enemyid, enemyBehaviorTreeGroup);
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
                    playerBase.tag = "PlayerBase" + playerid;
                }
                else
                {
                    //Only Assing Enemy Base Tag if mulitplayer
                    if(((RTSNetworkManager)NetworkManager.singleton).Players.Count > 1)
                        playerBase.tag = "PlayerBase" + enemyid;
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
    public IEnumerator TacticalFormation(int playerID, int enemyID)
    {
        yield return new WaitForSeconds(1f);
        GameObject[] armies = GameObject.FindGameObjectsWithTag("Player" + playerID);
        Dictionary<int , GameObject> leaders = new Dictionary<int, GameObject>();
        //Debug.Log($"TacticalFormation armies size {armies.Length} for player id {playerID} ");
        int i = 0;
        behaviorTreeGroups[playerID].Clear();
        int leaderUnitTypeID = 0;
        int randBase = 0;
        GameObject king = null;
        foreach (GameObject child in armies)
        {
            if (child.name.Contains("]"))
            {
                child.name = child.name.Substring(child.name.IndexOf("]") + 2 );
            }
            if (child.GetComponent<Unit>().unitType == Unit.UnitType.KING){
                king = child;
            } else if (!leaders.ContainsKey((int)child.GetComponent<Unit>().unitType)   )
            {
                leaders.Add((int) child.GetComponent<Unit>().unitType, child);
                child.name = "LEADER" + leaders.Count;
            }
            child.name = child.name.Length > 6 ? child.name.Substring(0, 6) : child.name;
            child.name = "[" + i + "]\t" + child.name;
            child.transform.parent = PlayerEnemyGroup[playerID].transform;
            i++;
        }
        List<KeyValuePair<int, GameObject>> leaderlist = leaders.ToList();
        //Debug.Log($"playerID {playerID} Heros size {heros.Count}");
        for (int j = 0; j < PlayerEnemyGroup[playerID].transform.childCount; ++j)
        {
            var child = PlayerEnemyGroup[playerID].transform.GetChild(j);
            leaderUnitTypeID = (int)child.GetComponent<Unit>().unitType;
            randBase = randBase == 0 ? 1 : 0;
            //Debug.Log($"randLeader {rr}/{randLeader}/{leaders.Count} randBase {randBase}");

            if (child.GetComponent<Unit>().unitType == Unit.UnitType.HERO ) {
                defendObject = king;
                defendObject.name = "King";
            }
            else {
                defendObject = GameObject.FindGameObjectsWithTag("PlayerBase" + playerID)[randBase];
                defendObject.name = "PlayerBase" + playerID + randBase;
            }
            var agentTrees = child.GetComponents<BehaviorTree>();
            for (int k = 0; k < agentTrees.Length; ++k)
            {
                var group = agentTrees[k].Group;
                if (group == 1 || group ==2 || group == 4 || group == 6 || group == 9 || group == 11 || group == 12) { continue; }
                agentTrees[k].SetVariableValue("newTargetName", "Player" + enemyID);
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
                if (!behaviorTreeGroups[playerID].TryGetValue(leaderUnitTypeID, out leaderBehaviorTrees)) {
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
        printTB();
        
        if (playerID == 0 || ((RTSNetworkManager)NetworkManager.singleton).Players.Count > 1)
            LeaderUpdated?.Invoke(leaders);

        setupLeaderID(playerID);
    }
    public void HandleLeaderSelected(int leaderId)
    {
        selectedLeaderId = leaderId;
    }
    public void TryTB(int type )
    {
        TryTB(type, playerid);
    }
    public void TryTB(int type, int playerID)
    {
        type = type % System.Enum.GetNames(typeof(BehaviorSelectionType)).Length;
        prevSelectionType = selectionType;
        selectionType = (BehaviorSelectionType)type;
        if (playerID == this.playerid)
            playerType = selectionType.ToString();
        else
            enemyType = selectionType.ToString();

        SelectionChanged(playerID);
    }
    public void TryReinforcePlayer(Unit unit)
    {
        if (unit.tag == ENEMYTAG) { return; }
        Debug.Log($"Auto Reinforce ..... {unit.name}");
        if(unit.name.ToLower().Contains("leader"))
            TryReinforce(playerid, enemyid);
    }
    public void TryReinforce(int playerID, int enemyID)
    {
        selectionType = prevSelectionType;
        // assign tag for new spawn instance, e.g. click on card
        StartCoroutine(AssignTag());
        // put unit under player / enemy behavior list
        StartCoroutine(TacticalFormation(playerID, enemyID));
        
        SelectionChanged(playerID);
    }
    
    private void SelectionChanged(int playerID)
    {
        //if (agentGroup is null) { return; }
        int leaderid = 0;
        if (playerID == 1 && ((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1) {
            leaderid = selectedEnemyLeaderId;
        } else
            leaderid = selectedLeaderId;
        StopCoroutine(EnableBehavior(playerID, leaderid));
        StartCoroutine(DisableBehavior(playerID, leaderid));
        StartCoroutine(EnableBehavior(playerID, leaderid));
        Eleixier.speedUpEleixier(GetBehaviorSelectionType());
    }
    private IEnumerator EnableBehavior(int playerID, int leaderID)
    {
        yield return new WaitForSeconds(0.1f);
        if(playerID == 0)
        Debug.Log($"EnableBehavior {selectionType} Unit Count { behaviorTreeGroups[playerID][leaderID][(int)selectionType].Count} for player ID {playerID} leader ID {leaderID}");
        for (int i = 0; i < behaviorTreeGroups[playerID][leaderID][(int)selectionType].Count; ++i)
        {
            if (behaviorTreeGroups[playerID][leaderID][(int)selectionType][i] != null)
            {
                behaviorTreeGroups[playerID][leaderID][(int)selectionType][i].EnableBehavior();
            }
        }
    }
    public IEnumerator DisableBehavior(int playerID, int leaderID)
    {
        yield return new WaitForSeconds(0.1f);
        //Debug.Log($"DisableBehavior playerID {playerID} leaderID {leaderID} ");
        for (int i = 0; i < behaviorTreeGroups[playerID][leaderID][(int)prevSelectionType].Count; ++i)
        {
            behaviorTreeGroups[playerID][leaderID][(int)prevSelectionType][i].DisableBehavior();
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
    public BehaviorSelectionType GetBehaviorSelectionType()
    {
        return selectionType;
    }
    void printTB()
    {
        StringBuilder sb = new StringBuilder();

        foreach (var ids in behaviorTreeGroups)
        {
            sb.Append($"Player Enemy ID {ids.Key} \n");
            foreach (var leaders in ids.Value)
            {
                sb.Append($"\t leader id {leaders.Key} \n");
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
    void setupLeaderID(int playerID)
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
    void AttackOnlyUnit(int playerID, int enemyID)
    {
        int leaderUnitTypeID = 99;
        for (int j = 0; j < PlayerEnemyGroup[playerID].transform.childCount; ++j)
        {
            var child = PlayerEnemyGroup[playerID].transform.GetChild(j);
            if (child.GetComponent<Unit>().unitType == Unit.UnitType.SPEARMAN)
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
    }
    #endregion
}
