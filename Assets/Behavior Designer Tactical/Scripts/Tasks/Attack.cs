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
        private UnitFactory localFactory;
        private int i = 0;
        public override TaskStatus OnUpdate()
        {
            var baseStatus = base.OnUpdate();
            if (baseStatus != TaskStatus.Running || !started) {
                return baseStatus;
            }
            tacticalAgent.transform.GetComponent<Unit>().SetTaskStatus("Attack : Searching target ");
            if (MoveToAttackPosition()) {
                tacticalAgent.transform.GetComponent<Unit>().SetTaskStatus("Attack : " + tacticalAgent.transform.GetComponent<battleFieldRules>().IsInField(tacticalAgent.transform)  + " " + tacticalAgent.TargetTransform.name + " (" + (int) (tacticalAgent.transform.position -  tacticalAgent.TargetTransform.position).sqrMagnitude + ")" );
                tacticalAgent.TryAttack();
            }

            if (base.leader.Value != null)
                base.leader.Value.GetComponent<HealthDisplay>().EnableLeaderIcon();
            if (localFactory == null)
            {
                foreach (GameObject factroy in GameObject.FindGameObjectsWithTag("UnitFactory"))
                {
                    if (factroy.GetComponent<UnitFactory>().hasAuthority)
                    {
                        localFactory = factroy.GetComponent<UnitFactory>();
                    }
                }
            }
            if (tacticalAgent.transform.GetComponent<Unit>().unitType == UnitMeta.UnitType.SPEARMAN && !tacticalAgent.transform.GetComponent<battleFieldRules>().IsInField(tacticalAgent.transform)&&i==0)
            {
                i++;
                localFactory.powerUp(tacticalAgent.transform.GetComponent<GameObject>(),3);

                Scale(tacticalAgent.transform.GetComponent<GameObject>());
                RpcScale(tacticalAgent.transform.GetComponent<GameObject>());
            }
            return TaskStatus.Running;
        }
        private void Scale(GameObject tacticalAgent)
        {
            tacticalAgent.transform.localScale = new Vector3(3, 3, 3);
        }
        [ClientRpc]
        private void RpcScale(GameObject tacticalAgent)
        {
            Scale(tacticalAgent);
        }
    }
}