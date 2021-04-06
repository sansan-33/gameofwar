using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using Tooltip = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;
using HelpURL = BehaviorDesigner.Runtime.Tasks.HelpURLAttribute;
using static Unit;
using Mirror;

namespace BehaviorDesigner.Runtime.Tactical.Tasks
{
    [TaskCategory("Tactical")]
    [TaskDescription("Moves to the closest target and starts attacking as soon as the agent is within distance")]
    [HelpURL("https://www.opsive.com/support/documentation/behavior-designer-tactical-pack/")]
    [TaskIcon("Assets/Behavior Designer Tactical/Editor/Icons/{SkinColor}AttackIcon.png")]
    public class Attack : NavMeshTacticalGroup
    {
        private int HEARTBEAT = 0;

        public override TaskStatus OnUpdate()
        {
            var baseStatus = base.OnUpdate();
            if (baseStatus != TaskStatus.Running || !started) {
                tacticalAgent.transform.GetComponent<Unit>().SetTaskStatus("Attack : TaskStatus " + baseStatus + " / " + HEARTBEAT++);
                return baseStatus;
            }
            tacticalAgent.transform.GetComponent<Unit>().SetTaskStatus("Attack : Searching target " + HEARTBEAT++);
            if (MoveToAttackPosition()) {
                tacticalAgent.transform.GetComponent<Unit>().SetTaskStatus("Attack : "  + tacticalAgent.TargetTransform.name + " (" + (int) (tacticalAgent.transform.position -  tacticalAgent.TargetTransform.position).sqrMagnitude + ") " + HEARTBEAT++);
                if(tacticalAgent.transform.GetComponent<Unit>().unitType == UnitMeta.UnitType.ARCHER) { Debug.Log("Archer attack"); }
                tacticalAgent.TryAttack();
            }

            //if (base.leader.Value != null)
            //    base.leader.Value.GetComponent<HealthDisplay>().EnableLeaderIcon();

            //tacticalAgent.transform.GetComponent<Unit>().GetUnitMovement().circleMarker.SetActive(false);

            return TaskStatus.Running;
        }
       
        
    }
}