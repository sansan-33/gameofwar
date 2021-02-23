
using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class TacticalBehavior : MonoBehaviour
{
    [SerializeField] List<GameObject> PlayerEnemyGroup = new List<GameObject>();
    public static event Action<List<GameObject>> LeaderUpdated;
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

    private List<GameObject> leaders = new List<GameObject>();
    private List<GameObject> leadersBackup = new List<GameObject>();

    private GameObject defendObject = null;
    public enum BehaviorSelectionType { Attack, Charge, MarchingFire, Flank, Ambush, ShootAndScoot, Leapfrog, Surround, Defend, Hold, Retreat, Reinforcements, Last }
    private BehaviorSelectionType selectionType = BehaviorSelectionType.Attack;
    private BehaviorSelectionType prevSelectionType = BehaviorSelectionType.Attack;
    

    private string playerType = "";
    private string enemyType = "";
    private Color teamColor;
    private Color teamEnemyColor;
    private int selectedLeaderId = 0;
    private int NOOFLEADERS = 2;
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
        List<GameObject> leaders = new List<GameObject>();
        List<GameObject> leadersBackup = new List<GameObject>();

        //Debug.Log($"TacticalFormation armies size {armies.Length} for player id {playerID} ");
        int i = 0;
        behaviorTreeGroups[playerID].Clear();
        int randLeader = 0;
        foreach (GameObject child in armies)
        {
            if (child.name.Contains("]"))
            {
                child.name = child.name.Substring(child.name.IndexOf("]") + 2 );
            }
            if (child.GetComponent<Unit>().unitType == Unit.UnitType.HERO){
                leaders.Add(child);
                child.name = "LEADER" + leaders.Count;
            }else if(leaders.Count == 0 && child.GetComponent<Unit>().unitType != Unit.UnitType.KING && leadersBackup.Count < NOOFLEADERS) {
                leadersBackup.Add(child);
                child.name = "LEADER" + leadersBackup.Count;
            }
            child.name = child.name.Length > 6 ? child.name.Substring(0, 6) : child.name;
            child.name = "[" + i + "]\t" + child.name;
            child.transform.parent = PlayerEnemyGroup[playerID].transform;
            i++;
        }
        if (leaders.Count == 0){
            leaders.Clear();
            foreach (GameObject gameObject in leadersBackup)
            {
                leaders.Add(gameObject);
            }
        }
        
        //Debug.Log($"playerID {playerID} Heros size {heros.Count}");
        for (int j = 0; j < PlayerEnemyGroup[playerID].transform.childCount; ++j)
        {
            var child = PlayerEnemyGroup[playerID].transform.GetChild(j);
            //rr.GetNextItem();
            randLeader = randLeader == 0 ? 1: 0; //rr.GetCurrentIndex(); 
            defendObject = GameObject.FindGameObjectsWithTag("PlayerBase" + playerID)[randLeader];
            defendObject.name = "PlayerBase" + playerID + randLeader;
            var agentTrees = child.GetComponents<BehaviorTree>();
            for (int k = 0; k < agentTrees.Length; ++k)
            {
                var group = agentTrees[k].Group;

                agentTrees[k].SetVariableValue("newTargetName", "Player" + enemyID);
                if (k == (int)BehaviorSelectionType.Hold || k == (int)BehaviorSelectionType.Defend)
                {
                    agentTrees[k].SetVariableValue("newDefendObject", defendObject);
                }
                if (!child.gameObject.name.ToUpper().Contains("LEADER") && leaders.Count >= randLeader + 1)
                {
                    agentTrees[k].SetVariableValue("newLeader", leaders[randLeader]);
                } else
                {
                    agentTrees[k].SetVariableValue("newLeader", null);
                }

                List<BehaviorTree> groupBehaviorTrees;
                Dictionary<int ,  List<BehaviorTree>> leaderBehaviorTrees;
                if (!behaviorTreeGroups[playerID].TryGetValue(randLeader, out leaderBehaviorTrees)) {
                    leaderBehaviorTrees = new Dictionary<int, List<BehaviorTree>>();
                    behaviorTreeGroups[playerID].Add(randLeader, leaderBehaviorTrees);
                }
                if (!behaviorTreeGroups[playerID][randLeader].TryGetValue(group, out groupBehaviorTrees))
                {
                    groupBehaviorTrees = new List<BehaviorTree>();
                    leaderBehaviorTrees.Add(group, groupBehaviorTrees);
                    //behaviorTreeGroups[playerID][randLeader].Add(group, groupBehaviorTrees);
                }
                groupBehaviorTrees.Add(agentTrees[k]);
                //if (playerID == 0)
                    //Debug.Log($" Leader Index {randLeader} tag {leaders[randLeader].tag} group {group} agent tree {k} {agentTrees[k]}");

            }
        }

        if(playerID == 0 || ((RTSNetworkManager)NetworkManager.singleton).Players.Count > 1)
            LeaderUpdated?.Invoke(leaders);
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

        StopCoroutine(EnableBehavior(playerID, selectedLeaderId));
        StartCoroutine(DisableBehavior(playerID, selectedLeaderId));
        StartCoroutine(EnableBehavior(playerID, selectedLeaderId));
        Eleixier.speedUpEleixier(GetBehaviorSelectionType());
    }
    private IEnumerator EnableBehavior(int playerID, int leaderID)
    {
        yield return new WaitForSeconds(0.1f);
        //Debug.Log($"EnableBehavior {selectionType} Unit Count { behaviorTreeGroups[playerID][(int)selectionType].Count} for player ID {playerID}");
        for (int i = 0; i < behaviorTreeGroups[playerID][leaderID][(int)selectionType].Count; ++i)
        {
            if (behaviorTreeGroups[playerID][leaderID][(int)selectionType][i] != null){
                behaviorTreeGroups[playerID][leaderID][(int)selectionType][i].EnableBehavior();
            }
        }
    }
    public IEnumerator DisableBehavior(int playerID, int leaderID)
    {
        yield return new WaitForSeconds(0.1f);
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
   
    #endregion
}
