using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using UnityEngine;

public class StupidTapir : MonoBehaviour
{
    // Start is called before the first frame update
    private Dictionary<int, List<BehaviorTree>> agentBehaviorTreeGroup = new Dictionary<int, List<BehaviorTree>>();
    void Start()
    { 
        
    }
    public void Attack()
    {
        int ID = tag.Contains("0") ? 1 : 0;
        var agentTrees = GetComponentsInChildren<BehaviorTree>();
        for (int j = 0; j < agentTrees.Length; ++j)
        {
            
            if(agentTrees[j].BehaviorName == "Attack")
            {
                agentTrees[j].SetVariableValue("newTargetName", "King" + ID);
                agentTrees[j].SetVariableValue("newLeader", null);
                agentTrees[j].EnableBehavior();
                break;
            }

           
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
