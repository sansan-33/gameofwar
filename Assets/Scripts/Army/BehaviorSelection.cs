using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Mirror;

public class BehaviorSelection :  NetworkBehaviour
{
        public GameObject agentGroup;
        private GameObject defendObject;
       
        private Dictionary<int, List<BehaviorTree>> agentBehaviorTreeGroup = new Dictionary<int, List<BehaviorTree>>();
      
        private enum BehaviorSelectionType { Attack, Charge, MarchingFire, Flank, Ambush, ShootAndScoot, Leapfrog, Surround, Defend, Hold, Retreat, Reinforcements, Last }
        private BehaviorSelectionType selectionType = BehaviorSelectionType.Attack;
        private BehaviorSelectionType prevSelectionType = BehaviorSelectionType.Attack;
        private RTSPlayer player;
        private int playerid = 0;
        private int enemyid = 0;
        private string enemyTag = "";

        void Start()
        {
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
            playerid = player.GetPlayerID();
            enemyid = playerid == 0 ? 1 : 0;
            enemyTag = "Player" + enemyid;
            Debug.Log($" BehaviorSelection --> player id {playerid} / enemyTag {enemyTag}");
            StartCoroutine("AssignTagTB");
        }

        private void startMilitaryTB()
        {

            GameObject hero=null;
            
            GameObject[] armies = GameObject.FindGameObjectsWithTag("Player" + playerid);
            Debug.Log($"2 Object with Player tag   {armies.Length} ");

            foreach (GameObject child in armies)
            {
                if (child.GetComponent<Unit>().hasAuthority)
                {
                    if (child.gameObject.name.ToUpper().Contains("HERO")) { hero = child; }
                    child.transform.parent = agentGroup.transform;
                }
            }

            Debug.Log($"agentGroup. .childCount  {agentGroup.transform.childCount} ");
            
            for (int i = 0; i < agentGroup.transform.childCount; ++i)
            {
                var child = agentGroup.transform.GetChild(i);
                Debug.Log($" {i} {child} ");
                var agentTrees = child.GetComponents<BehaviorTree>();
                for (int j = 0; j < agentTrees.Length; ++j)
                {
                    var group = agentTrees[j].Group;

                    agentTrees[j].SetVariableValue("newTargetName", enemyTag);
                    if (j == (int)BehaviorSelectionType.Hold || j == (int)BehaviorSelectionType.Defend)
                    {
                        agentTrees[j].SetVariableValue("newDefendObject", defendObject);
                    }
                    if (!child.gameObject.name.ToUpper().Contains("HERO")) {
                        agentTrees[j].SetVariableValue("newLeader", hero);
                    }
                    else
                    {
                        agentTrees[j].SetVariableValue("newLeader", null);
                    }

                    List<BehaviorTree> groupBehaviorTrees;
                    if (!agentBehaviorTreeGroup.TryGetValue(group, out groupBehaviorTrees))
                    {
                        groupBehaviorTrees = new List<BehaviorTree>();
                        agentBehaviorTreeGroup.Add(group, groupBehaviorTrees);
                    }
                    groupBehaviorTrees.Add(agentTrees[j]);
                }
            }

        }

        private string Description()
        {
            string desc = "";
            switch (selectionType) {
                case BehaviorSelectionType.Attack:
                    desc = "Moves to the closest target and starts attacking as soon as the agent is within distance.";
                    break;
                case BehaviorSelectionType.Charge:
                    desc = "Charges towards the target. The agents will start attacking when they are done charging.";
                    break;
                case BehaviorSelectionType.MarchingFire:
                    desc = "Move towards the target. The agents will start attacking when they are within distance.";
                    break;
                case BehaviorSelectionType.Flank:
                    desc = "Flanks the target from the left and right.";
                    break;
                case BehaviorSelectionType.Ambush:
                    desc = "Wait for the group of targets to pass before attacking.";
                    break;
                case BehaviorSelectionType.ShootAndScoot:
                    desc = "Attacks the target and moves position after a short amount of time.";
                    break;
                case BehaviorSelectionType.Leapfrog:
                    desc = "Search for the target by forming two groups and leapfrogging each other. Both groups will start attacking as soon as the target is within sight";
                    break;
                case BehaviorSelectionType.Surround:
                    desc = "Surrounds the enemy and starts to attack after all agents are in position";
                    break;
                case BehaviorSelectionType.Retreat:
                    desc = "Retreats in the opposite direction of the target";
                    break;
                case BehaviorSelectionType.Defend:
                    desc = "Defends the object within a defend radius. Will seek and attack a target within a specified radius";
                    break;
                case BehaviorSelectionType.Hold:
                    desc = "Defends the object within a defend radius. Will seek and attack a target for as long as it takes";
                    break;
                case BehaviorSelectionType.Reinforcements:
                    desc = "The attacking agent will request for reinforcements after waiting a moment. " + 
                           "The reinforcement agents will move towards the requesting agent and start attacking the targets when within distnace.";
                    break;
            }
            return desc;
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
        public void TryAmbush()
        {
            prevSelectionType = selectionType;
            selectionType = BehaviorSelectionType.Ambush;
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

    private void SelectionChanged()
        {
            StopCoroutine("EnableBehavior");
            for (int i = 0; i < agentBehaviorTreeGroup[(int)prevSelectionType].Count; ++i) {
                agentBehaviorTreeGroup[(int)prevSelectionType][i].DisableBehavior();
            }
            StartCoroutine("EnableBehavior");
        }

    private IEnumerator EnableBehavior()
    {
        //defendObject.SetActive(false);

        yield return new WaitForSeconds(0.1f);
        GameObject[] armies = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject army in armies)
        {
            army.GetComponent<Unit>().GetUnitMovement().unitNetworkAnimator.SetTrigger("wait");

        }


        //Debug.Log($"(int)selectionType {(int)selectionType} agentBehaviorTreeGroup count {agentBehaviorTreeGroup.Count} ");
        for (int i = 0; i < agentBehaviorTreeGroup[(int)selectionType].Count; ++i)
        {
            agentBehaviorTreeGroup[(int)selectionType][i].EnableBehavior();
            //Debug.Log($"(int)selectionType {(int)selectionType} / {i} ==== {agentBehaviorTreeGroup[(int)selectionType][i]}");
        }

        foreach (GameObject army in armies)
        {
            army.GetComponent<Unit>().GetUnitMovement().unitNetworkAnimator.SetTrigger("run");

        }
    }
    private IEnumerator AssignTagTB()
    {
        yield return new WaitForSeconds(10f);
        GameObject[] playerBases = GameObject.FindGameObjectsWithTag("PlayerBase");
        foreach (GameObject playerBase in playerBases)
        {
            if (playerBase.TryGetComponent<Unit>(out Unit unit))
            {
                if (unit.hasAuthority)
                {
                    playerBase.tag = "PlayerBase" + playerid;
                    defendObject = playerBase;
                    Debug.Log($"1.1.1 Defend Object | {defendObject} | playerBase {playerBase }? ");

                }
                else
                {
                    playerBase.tag = "PlayerBase" + enemyid;
                }
            }
        }
        yield return new WaitForSeconds(1f);

        //defendObject = GameObject.FindGameObjectWithTag("PlayerBase" + playerid);
        Debug.Log($"1.1 Defend Object | {defendObject} | ? ");


        GameObject[] armies = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log($"AssignTagTB --> Armies Size: {armies.Length }");
        foreach (GameObject army in armies)
        {
            if (army.TryGetComponent<Unit>(out Unit unit))
            {
                Debug.Log($"Checking unit: {unit } / unit.hasAuthority {unit.hasAuthority}");

                if (unit.hasAuthority) {
                    army.tag = "Player" + playerid;
                }
                else {
                    army.tag = enemyTag;
                }
            }
        }
        yield return new WaitForSeconds(3f);
        startMilitaryTB();
    }
        
}
 