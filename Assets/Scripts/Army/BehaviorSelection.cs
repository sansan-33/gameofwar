using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BehaviorDesigner.Runtime;

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

        public void Start()
        {
            InvokeRepeating("addBehaviourToMilitary", 5f, 6000000f);
        }
        private void addBehaviourToMilitary()
        {


            defendObject = GameObject.FindGameObjectWithTag("EnemyBase");
            GameObject[] armies = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject child in armies)
            {
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
                    List<BehaviorTree> groupBehaviorTrees;
                    if (!agentBehaviorTreeGroup.TryGetValue(group, out groupBehaviorTrees))
                    {
                        groupBehaviorTrees = new List<BehaviorTree>();
                        agentBehaviorTreeGroup.Add(group, groupBehaviorTrees);
                    }
                    groupBehaviorTrees.Add(agentTrees[j]);
                }
            }
            //enemyHealth = enemyGroup.GetComponentsInChildren<Health>();


            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject child in enemies)
            {
                child.transform.parent =  enemyGroup.transform;
            }
            var behaviorTrees = enemyGroup.GetComponentsInChildren<BehaviorTree>();
            for (int i = 0; i < behaviorTrees.Length; ++i)
            {
                List<BehaviorTree> list;
                //Debug.Log($" {i} {behaviorTrees[i]} ");
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
            /*
            if (enemyBehaviorTreeGroup.ContainsKey((int)prevSelectionType)) {
                var trees = enemyBehaviorTreeGroup[(int)prevSelectionType];
                for (int i = 0; i < trees.Count; ++i) {
                    trees[i].DisableBehavior();
                }
            }
            */
            StartCoroutine("EnableBehavior");
        }

        private static string SplitCamelCase(string s)
        {
            var r = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])|(?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);
            s = r.Replace(s, " ");
            return (char.ToUpper(s[0]) + s.Substring(1)).Trim();
        }

        private IEnumerator EnableBehavior()
        {
            defendObject.SetActive(false);
      
            yield return new WaitForSeconds(0.1f);
        /*
            for (int i = 0; i < enemyHealth.Length; ++i) {
                enemyHealth[i].ResetHealth();
            }
        
            if (enemyBehaviorTreeGroup.ContainsKey((int)selectionType)) {
                var trees = enemyBehaviorTreeGroup[(int)selectionType];
                for (int i = 0; i < trees.Count; ++i) {
                    //Debug.Log($"trees {i} {trees[i]}");
                    trees[i].EnableBehavior();
                }
            }
        */
        //Debug.Log($"(int)selectionType {(int)selectionType} agentBehaviorTreeGroup count {agentBehaviorTreeGroup.Count} ");
        for (int i = 0; i < agentBehaviorTreeGroup[(int)selectionType].Count; ++i) {
                agentBehaviorTreeGroup[(int)selectionType][i].EnableBehavior();
            }
        }

        private void SetPosRot(Vector3 agentGroupPosition, Vector3 agentGroupRotation, Vector3 enemyGroupPosition, Vector3 enemyGroupRotation, Vector3 cameraPosition)
        {
            //Debug.Log($"SetPosRot : {agentGroupPosition} ");
            agentGroup.transform.position = agentGroupPosition;
            agentGroup.transform.eulerAngles = agentGroupRotation;
            enemyGroup.transform.position = enemyGroupPosition;
            enemyGroup.transform.eulerAngles = enemyGroupRotation;
            Camera.main.transform.position = cameraPosition;
        }

        private void SetChildPosRot(Vector3[] agentPositions, Vector3[] agentRotations, Vector3[] enemyPositions, Vector3[] enemyRotations)
        {
            /*
            for (int i = 0; i < agentGroup.transform.childCount; ++i) {
                var child = agentGroup.transform.Find("Agent " + (i + 1));
                child.localPosition = agentPositions[i];
                child.localEulerAngles = agentRotations[i];
            }
            */
            int j = 0;
            foreach (Transform child in agentGroup.transform)
            {
                //var child = agentGroup.transform.Find("Agent " + (i + 1));
                child.localPosition = agentPositions[j];
                child.localEulerAngles = agentRotations[j];
                j++;
            }
            for (int i = 0; i < enemyGroup.transform.childCount; ++i) {
                var child = enemyGroup.transform.Find("Enemy " + (i + 1));
                child.localPosition = enemyPositions[i];
                child.localEulerAngles = enemyRotations[i];
            }
        }
    }
