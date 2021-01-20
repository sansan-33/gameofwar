using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using Tooltip = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;
using HelpURL = BehaviorDesigner.Runtime.Tasks.HelpURLAttribute;

namespace BehaviorDesigner.Runtime.Tactical.Tasks
{
    [TaskCategory("Tactical")]
    [TaskDescription("Defends the object within a defend radius. Will seek and attack a target within a specified radius")]
    [HelpURL("https://www.opsive.com/support/documentation/behavior-designer-tactical-pack/")]
    [TaskIcon("Assets/Behavior Designer Tactical/Editor/Icons/{SkinColor}DefendIcon.png")]
    public class Defend : NavMeshTacticalGroup
    {
        [Tooltip("The object to defend")]
        public SharedGameObject defendObject;
        [Tooltip("The radius around the defend object to position the agents")]
        public SharedFloat radius = 3;
        [Tooltip("The radius around the defend object to defend")]
        public SharedFloat defendRadius = 10;
        [Tooltip("The maximum distance that the agents can defend from the defend object")]
        public SharedFloat maxDistance = 15;

        private float theta;
        private string TASKNAME = "Defend";

        protected override void AddAgentToGroup(Behavior agent, int index)
        {
            base.AddAgentToGroup(agent, index);

            // 2 * PI = 360 degrees
            theta = 2 * Mathf.PI / agents.Count;
        }

        protected override int RemoveAgentFromGroup(Behavior agent)
        {
            var index = base.RemoveAgentFromGroup(agent);

            // 2 * PI = 360 degrees
            theta = 2 * Mathf.PI / agents.Count;

            return index;
        }

        public override TaskStatus OnUpdate()
        {
            var baseStatus = base.OnUpdate();
            if (baseStatus != TaskStatus.Running || !started) {
                return baseStatus;
            }

            // Attack the target if the agent has a target.
            if (tacticalAgent.TargetTransform != null) {
                // Stop attacking if the target gets too far away from the defend object.
                if ((transform.position - defendObject.Value.transform.position).magnitude > maxDistance.Value || !tacticalAgent.TargetDamagable.IsAlive()) {
                    tacticalAgent.TargetTransform = null;
                    tacticalAgent.TargetDamagable = null;
                    tacticalAgent.AttackPosition = false;
                    tacticalAgent.transform.GetComponent<Unit>().SetTaskStatus(TASKNAME + ": target gets too far away from the defend object");
                }
                else {
                    // The target is within distance. Keep moving towards it.
                    tacticalAgent.AttackPosition = true;
                    if (MoveToAttackPosition()) {
                        tacticalAgent.transform.GetComponent<Unit>().SetTaskStatus(TASKNAME +  ": Attack");
                        tacticalAgent.TryAttack();
                    }
                }
            } else {
                // Loop through the possible target transforms and determine which transform is the closest to each agent.
                tacticalAgent.transform.GetComponent<Unit>().SetTaskStatus(TASKNAME + ": searching target");
                for (int i = targetTransforms.Count - 1; i > -1; --i) {
                    // The target has to be alive.
                    if (targets[i].IsAlive()) {
                        // Start attacking if the target gets too close.
                        if ((transform.position - targetTransforms[i].position).magnitude < defendRadius.Value) {
                            tacticalAgent.TargetDamagable = targets[i];
                            tacticalAgent.TargetTransform = targetTransforms[i];
                        }
                    } else {
                        // The target is no longer alive - remove it from the list.
                        targets.RemoveAt(i);
                        targetTransforms.RemoveAt(i);
                    }
                }
            }

            // The agent isn't attacking. Move near the defend object.
            if (!tacticalAgent.AttackPosition) {
                var targetPosition = defendObject.Value.transform.TransformPoint(radius.Value * Mathf.Sin(theta * formationIndex), 0, radius.Value * Mathf.Cos(theta * formationIndex));
                tacticalAgent.UpdateRotation(true);
                tacticalAgent.transform.GetComponent<Unit>().SetTaskStatus(TASKNAME + " = " + ": moving distance " + (int) Vector3.Distance(tacticalAgent.transform.position, targetPosition) );
                tacticalAgent.SetDestination(targetPosition);
                if (tacticalAgent.HasArrived()) {
                    // Face away from the defending object.
                    var direction = targetPosition - defendObject.Value.transform.position;
                    direction.y = 0;
                    tacticalAgent.RotateTowards(Quaternion.LookRotation(direction));
                    tacticalAgent.transform.GetComponent<Unit>().SetTaskStatus( TASKNAME + ": Arrived");
                    tacticalAgent.transform.GetComponent<Unit>().GetUnitMovement().CmdTrigger("defend");
                }
            }

            return TaskStatus.Running;
        }

        public override void OnReset()
        {
            base.OnReset();

            defendObject = null;
            radius = 5;
            defendRadius = 10;
        }
    }
}