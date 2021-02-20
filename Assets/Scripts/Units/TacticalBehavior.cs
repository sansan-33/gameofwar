
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
    private eleixier Eleixier;
    private RTSPlayer player;
    private int playerid = 0;
    private int enemyid = 0;
    private string ENEMYTAG = "";
    private string PLAYERTAG = "";
    private Dictionary<int, List<BehaviorTree>> playerBehaviorTreeGroup = new Dictionary<int, List<BehaviorTree>>();
    private Dictionary<int, List<BehaviorTree>> enemyBehaviorTreeGroup = new Dictionary<int, List<BehaviorTree>>();
    private Dictionary<int, Dictionary<int, List<BehaviorTree>>> behaviorTreeGroups = new Dictionary < int, Dictionary<int, List<BehaviorTree>>>();
    private GameObject defendObject = null;
    public enum BehaviorSelectionType { Attack, Charge, MarchingFire, Flank, Ambush, ShootAndScoot, Leapfrog, Surround, Defend, Hold, Retreat, Reinforcements, Last }
    private BehaviorSelectionType selectionType = BehaviorSelectionType.Attack;
    private BehaviorSelectionType prevSelectionType = BehaviorSelectionType.Attack;

    private string playerType = "";
    private string enemyType = "";
    private Color teamColor;
    private Color teamEnemyColor;
    
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
    }
    public void OnDestroy()
    {
        Unit.ServerOnUnitDespawned -= TryReinforcePlayer;
        Unit.ServerOnUnitSpawned -= TryReinforcePlayer;
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
        List<GameObject> heros = new List<GameObject>();
        GameObject[] armies = GameObject.FindGameObjectsWithTag("Player" + playerID);
        //Debug.Log($"TacticalFormation armies size {armies.Length} for player id {playerID} ");
        int i = 0;
        behaviorTreeGroups[playerID].Clear();
        int rand = 0;
        foreach (GameObject child in armies)
        {
            // Set Higher Priority value will avoid lower value 
            //child.GetComponent<NavMeshAgent>().avoidancePriority = 70;
            //if (child.GetComponent<Unit>().hasAuthority)
            //{
            if (child.name.Contains("]"))
            {
                child.name = child.name.Substring(child.name.IndexOf("]") + 2 );
            }
            if (child.GetComponent<Unit>().unitType == Unit.UnitType.HERO){
                heros.Add(child);
                child.name = "LEADER" + heros.Count;
            }
            child.name = child.name.Length > 6 ? child.name.Substring(0, 6) : child.name;
            child.name = "[" + i + "]\t" + child.name;
            child.transform.parent = PlayerEnemyGroup[playerID].transform;
            i++;
            //}
        }
        //Debug.Log($"playerID {playerID} Heros size {heros.Count}");
        for (int j = 0; j < PlayerEnemyGroup[playerID].transform.childCount; ++j)
        {
            var child = PlayerEnemyGroup[playerID].transform.GetChild(j);
            rand = rand == 0 ? 1 : 0; 
            defendObject = GameObject.FindGameObjectsWithTag("PlayerBase" + playerID)[rand];
            defendObject.name = "PlayerBase" + playerID + rand;
            //Debug.Log($" {i} {child} ");
            var agentTrees = child.GetComponents<BehaviorTree>();
            for (int k = 0; k < agentTrees.Length; ++k)
            {
                var group = agentTrees[k].Group;

                agentTrees[k].SetVariableValue("newTargetName", "Player" + enemyID);
                if (k == (int)BehaviorSelectionType.Hold || k == (int)BehaviorSelectionType.Defend)
                {
                    agentTrees[k].SetVariableValue("newDefendObject", defendObject);
                }
                if (!child.gameObject.name.ToUpper().Contains("LEADER"))
                {
                    agentTrees[k].SetVariableValue("newLeader", heros[rand]);
                }
                else
                {
                    agentTrees[k].SetVariableValue("newLeader", null);
                }

                List<BehaviorTree> groupBehaviorTrees;
                if (!behaviorTreeGroups[playerID].TryGetValue(group, out groupBehaviorTrees))
                {
                    groupBehaviorTrees = new List<BehaviorTree>();
                    behaviorTreeGroups[playerID].Add(group, groupBehaviorTrees);
                }
                groupBehaviorTrees.Add(agentTrees[k]);
            }
        }
        
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

        StopCoroutine(EnableBehavior(playerID));
        StartCoroutine(DisableBehavior(playerID));
        StartCoroutine(EnableBehavior(playerID));
        Eleixier.speedUpEleixier(GetBehaviorSelectionType());
    }
    private IEnumerator EnableBehavior(int playerID)
    {
        yield return new WaitForSeconds(0.1f);
        //Debug.Log($"EnableBehavior {selectionType} Unit Count { behaviorTreeGroups[playerID][(int)selectionType].Count} for player ID {playerID}");
        for (int i = 0; i < behaviorTreeGroups[playerID][(int)selectionType].Count; ++i)
        {
            if (behaviorTreeGroups[playerID][(int)selectionType][i] != null)
                behaviorTreeGroups[playerID][(int)selectionType][i].EnableBehavior();
        }
    }
    public IEnumerator DisableBehavior(int playerID)
    {
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < behaviorTreeGroups[playerID][(int)prevSelectionType].Count; ++i)
        {
            behaviorTreeGroups[playerID][(int)prevSelectionType][i].DisableBehavior();
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

    public void TryAttack()
    {
        TryAttack(playerid);
    }
    public void TryDefend()
    {
        TryDefend(playerid);
    }
    public void TryRetreat()
    {
        TryRetreat(playerid);
    }
    public void TryFlank()
    {
        TryFlank(playerid);
    }
    public void TrySurround()
    {
        TrySurround(playerid);
    }
    public void TryShootAndScoot()
    {
        TryShootAndScoot(playerid);
    }
    public void TryAttack(int playerID)
    {
        TryTB((int)BehaviorSelectionType.Attack, playerID);
    }
    public void TryDefend(int playerID)
    {
        TryTB((int)BehaviorSelectionType.Defend, playerID);
    }
    public void TryRetreat(int playerID)
    {
        TryTB((int)BehaviorSelectionType.Retreat, playerID);

    }
    public void TryFlank(int playerID)
    {
        TryTB((int)BehaviorSelectionType.Flank, playerID);
    }
    public void TrySurround(int playerID)
    {
        TryTB((int)BehaviorSelectionType.Surround, playerID);
    }
    public void TryShootAndScoot(int playerID)
    {
        TryTB((int)BehaviorSelectionType.ShootAndScoot, playerID);
    }
    #endregion
}
