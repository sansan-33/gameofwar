
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using Mirror;
using UnityEngine;

public class TacticalBehavior : MonoBehaviour
{
    [SerializeField] public GameObject agentGroup = null;

    private RTSPlayer player;
    private int playerid = 0;
    private int enemyid = 0;
    private string ENEMYTAG = "";
    private string PLAYERTAG = "";
    private Dictionary<int, List<BehaviorTree>> agentBehaviorTreeGroup = new Dictionary<int, List<BehaviorTree>>();
    private GameObject defendObject = null;
    private enum BehaviorSelectionType { Attack, Charge, MarchingFire, Flank, Ambush, ShootAndScoot, Leapfrog, Surround, Defend, Hold, Retreat, Reinforcements, Last }
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
        StartCoroutine("AssignTagTB");
    }

    private IEnumerator AssignTagTB()
    {
        yield return new WaitForSeconds(1f);
        GameObject[] playerBases = GameObject.FindGameObjectsWithTag("PlayerBase");
        foreach (GameObject playerBase in playerBases)
        {
            if (playerBase.TryGetComponent<Unit>(out Unit unit))
            {
                if (unit.hasAuthority)
                {
                    playerBase.tag = "PlayerBase" + playerid;
                    defendObject = playerBase;
                }
                else
                {
                    playerBase.tag = "PlayerBase" + enemyid;
                }
            }
        }

        yield return new WaitForSeconds(1f);

        GameObject[] armies = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject army in armies)
        {
            if (army.TryGetComponent<Unit>(out Unit unit))
            {
                if (unit.hasAuthority) { army.tag = PLAYERTAG; }
                else { army.tag = ENEMYTAG; }
            }
        }
        startMilitaryTB();
        yield return new WaitForSeconds(0.1f);
    }
    private void startMilitaryTB()
    {
        GameObject hero = null;
        GameObject[] armies = GameObject.FindGameObjectsWithTag(PLAYERTAG);
        int i = 0;
        foreach (GameObject child in armies)
        {
            if (child.GetComponent<Unit>().hasAuthority)
            {
                if (i == 0)
                {
                    hero = child;
                    hero.name = "LEADER";
                }
                child.transform.parent = agentGroup.transform;
                i++;
            }
        }

        for (int j = 0; j < agentGroup.transform.childCount; ++j)
        {
            var child = agentGroup.transform.GetChild(j);
            //Debug.Log($" {i} {child} ");
            var agentTrees = child.GetComponents<BehaviorTree>();
            for (int k = 0; k < agentTrees.Length; ++k)
            {
                var group = agentTrees[k].Group;

                agentTrees[k].SetVariableValue("newTargetName", ENEMYTAG);
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
                if (!agentBehaviorTreeGroup.TryGetValue(group, out groupBehaviorTrees))
                {
                    groupBehaviorTrees = new List<BehaviorTree>();
                    agentBehaviorTreeGroup.Add(group, groupBehaviorTrees);
                }
                groupBehaviorTrees.Add(agentTrees[k]);
            }
        }

    }
    public void TryAttack()
    {
        prevSelectionType = selectionType;
        selectionType = BehaviorSelectionType.Attack;
        SelectionChanged();
    }
    public void TryDefend()
    {
        prevSelectionType = selectionType;
        selectionType = BehaviorSelectionType.Defend;
        SelectionChanged();
    }
    public void TryRetreat()
    {
        prevSelectionType = selectionType;
        selectionType = BehaviorSelectionType.Retreat;
        SelectionChanged();

    }
    public void TryFlank()
    {
        prevSelectionType = selectionType;
        selectionType = BehaviorSelectionType.Flank;
        SelectionChanged();

    }
    public void TrySurround()
    {
        prevSelectionType = selectionType;
        selectionType = BehaviorSelectionType.Surround;
        SelectionChanged();

    }
    public void TryTB(int type)
    {
        type = type % System.Enum.GetNames(typeof(BehaviorSelectionType)).Length;
        prevSelectionType = selectionType;
        selectionType = (BehaviorSelectionType)type;
        SelectionChanged();
    }
    
    private void SelectionChanged()
    {
        if (agentGroup is null) { return; }

        StopCoroutine("EnableBehavior");
        StartCoroutine("DisableBehavior");
        StartCoroutine("EnableBehavior");
    }
    private IEnumerator EnableBehavior()
    {
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < agentBehaviorTreeGroup[(int)selectionType].Count; ++i)
        {
            if (agentBehaviorTreeGroup[(int)selectionType][i] != null)
                agentBehaviorTreeGroup[(int)selectionType][i].EnableBehavior();
        }
    }
    public IEnumerator DisableBehavior()
    {
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < agentBehaviorTreeGroup[(int)prevSelectionType].Count; ++i)
        {
            agentBehaviorTreeGroup[(int)prevSelectionType][i].DisableBehavior();
        }

    }
    #endregion
}
