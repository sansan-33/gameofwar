using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class BehaviorSelection : MonoBehaviour
    {
        public GameObject agentGroup;
        public GameObject enemyGroup;
        public GameObject defendObject;
       
        private Dictionary<int, List<BehaviorTree>> agentBehaviorTreeGroup = new Dictionary<int, List<BehaviorTree>>();
        private Dictionary<int, List<BehaviorTree>> enemyBehaviorTreeGroup = new Dictionary<int, List<BehaviorTree>>();
        private Health[] enemyHealth;

        private enum BehaviorSelectionType { Attack, Charge, MarchingFire, Flank, Ambush, ShootAndScoot, Leapfrog, Surround, Defend, Hold, Retreat, Reinforcements, Last }
        private BehaviorSelectionType selectionType = BehaviorSelectionType.Attack;
        private BehaviorSelectionType prevSelectionType = BehaviorSelectionType.Attack;

        public void Start ()
        {
            InvokeRepeating("addBehaviourToMilitary", 5f, 6000000f);
        }
        private void addBehaviourToMilitary()
        {


            GameObject hero=null;
            defendObject = GameObject.FindGameObjectWithTag("EnemyBase");
            GameObject[] armies = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject child in armies)
            {
                if (child.gameObject.name.Contains("Hero")) { hero = child; }
                child.transform.parent = agentGroup.transform;
            }
        
            for (int i = 0; i < agentGroup.transform.childCount; ++i)
            {
                var child = agentGroup.transform.GetChild(i);
                //Debug.Log($" {i} {child} ");
                var agentTrees = child.GetComponents<BehaviorTree>();
            for (int j = 0; j < agentTrees.Length; ++j)
            {
                var group = agentTrees[j].Group;

                if (!child.gameObject.name.Contains("Hero")) {
                    agentTrees[j].SetVariableValue("newLeader", hero);
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
       

            enemyHealth = enemyGroup.GetComponentsInChildren<Health>();
            var behaviorTrees = enemyGroup.GetComponentsInChildren<BehaviorTree>();
            for (int i = 0; i < behaviorTrees.Length; ++i)
            {
                List<BehaviorTree> list;
                if (enemyBehaviorTreeGroup.TryGetValue(behaviorTrees[i].Group, out list))
                {
                    list.Add(behaviorTrees[i]);
                }
                else
                {
                    list = new List<BehaviorTree>();
                    list.Add(behaviorTrees[i]);
                    enemyBehaviorTreeGroup[behaviorTrees[i].Group] = list;
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
            
            if (enemyBehaviorTreeGroup.ContainsKey((int)prevSelectionType)) {
                var trees = enemyBehaviorTreeGroup[(int)prevSelectionType];
                for (int i = 0; i < trees.Count; ++i) {
                    trees[i].DisableBehavior();
                }
            }
          
            StartCoroutine("EnableBehavior");
        }

        private IEnumerator EnableBehavior()
        {
            //defendObject.SetActive(false);

            yield return new WaitForSeconds(0.1f);
        
            if (enemyBehaviorTreeGroup.ContainsKey((int)selectionType)) {
                var trees = enemyBehaviorTreeGroup[(int)selectionType];
                for (int i = 0; i < trees.Count; ++i) {
                    //Debug.Log($"trees {i} {trees[i]}");
                    trees[i].EnableBehavior();
                }
            }
        
            //Debug.Log($"(int)selectionType {(int)selectionType} agentBehaviorTreeGroup count {agentBehaviorTreeGroup.Count} ");
            for (int i = 0; i < agentBehaviorTreeGroup[(int)selectionType].Count; ++i) {
                agentBehaviorTreeGroup[(int)selectionType][i].EnableBehavior();
                //Debug.Log($"(int)selectionType {(int)selectionType} / {i} ==== {agentBehaviorTreeGroup[(int)selectionType][i]}");
            }
        }

        

        
}
