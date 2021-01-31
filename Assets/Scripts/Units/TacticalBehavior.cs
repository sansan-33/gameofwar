
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using Mirror;
using UnityEngine;

public class TacticalBehavior : MonoBehaviour
{
    [SerializeField] List<GameObject> group = new List<GameObject>();

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

    #region Client

    public void Awake()
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        playerid = player.GetPlayerID();
        enemyid = player.GetEnemyID();
        ENEMYTAG = "Player" + enemyid;
        PLAYERTAG = "Player" + playerid;
        StartCoroutine(AssignTag());
        StartCoroutine(TacticalFormation(playerid, enemyid));
        behaviorTreeGroups.Add(playerid, playerBehaviorTreeGroup);
        behaviorTreeGroups.Add(enemyid, enemyBehaviorTreeGroup);

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
                    defendObject = playerBase;
                }
                else
                {
                    //Only Assing Enemy Base Tag if mulitplayer
                    if(FindObjectOfType<NetworkManager>().numPlayers > 1)
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
                if (unit.hasAuthority) { army.tag = PLAYERTAG; }
                else {
                    //Only Assing Enemy Base Tag if mulitplayer
                    //Debug.Log($"Army Enemy Tag , Number of player {FindObjectOfType<NetworkManager>().numPlayers } ");
                    //if (FindObjectOfType<NetworkManager>().numPlayers > 1)
                        army.tag = ENEMYTAG;
                }
            }
        }

    }
    public IEnumerator TacticalFormation(int playerID, int enemyID)
    {
        yield return new WaitForSeconds(3f);
        GameObject hero = null;
        GameObject[] armies = GameObject.FindGameObjectsWithTag("Player" + playerID);
        //Debug.Log($"TacticalFormation armies size {armies.Length} for player id {playerID} ");
        int i = 0;
        defendObject = GameObject.FindGameObjectWithTag("PlayerBase" + playerID);
        
        foreach (GameObject child in armies)
        {
            //if (child.GetComponent<Unit>().hasAuthority)
            //{
                if (i == 0)
                {
                    hero = child;
                    hero.name = "LEADER";
                }
                child.transform.parent = group[playerID].transform;
                i++;
            //}
        }

        for (int j = 0; j < group[playerID].transform.childCount; ++j)
        {
            var child = group[playerID].transform.GetChild(j);
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
                    agentTrees[k].SetVariableValue("newLeader", hero);
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
        //Debug.Log($"Try TB playerID {playerID} {selectionType.ToString()}");
        SelectionChanged(playerID);
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

    #endregion
}
