using UnityEngine;
using UnityEngine.AI;
using HelpURL = BehaviorDesigner.Runtime.Tasks.HelpURLAttribute;

namespace BehaviorDesigner.Runtime.Tactical.Tasks
{
    /// <summary>
    /// Base class for all NavMeshAgent Tactical tasks.
    /// </summary>
    public abstract class NavMeshTacticalGroup : TacticalGroup
    {
        /// <summary>
        /// The NavMeshTacticalAgent class contains component references and variables for each NavMeshAgent.
        /// </summary>
        private class NavMeshTacticalAgent : TacticalAgent
        {
            private Unit unit;
            private bool destinationSet;
           
            /// <summary>
            /// Caches the component references and initialize default values.
            /// </summary>
            public NavMeshTacticalAgent(Transform agent) : base(agent)
            {
                //navMeshAgent = agent.GetComponent<NavMeshAgent>();
                unit = agent.GetComponentInParent<Unit>();
                //if (navMeshAgent.hasPath) {
                //    navMeshAgent.ResetPath();
                //    navMeshAgent.isStopped = true;
                //}
            }

            /// <summary>
            /// Sets the destination.
            /// </summary>
            public override void SetDestination(Vector3 destination)
            {
                unit.GetUnitMovement().move(destination);
            }
            /// <summary>
            /// Has the agent arrived at its destination?
            /// </summary>
            public override bool HasArrived()
            {
                if (unit == null) { return false; }
                return unit.GetUnitMovement().hasArrived();
            }

            /// <summary>
            /// Rotates towards the target rotation.
            /// </summary>
            public override bool RotateTowards(Quaternion targetRotation)
            {
                Vector3 eulerRotation = targetRotation.eulerAngles;
                targetRotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, 0); // fix the unit rotate z axis like falling down
                unit.GetUnitMovement().rotate(targetRotation);
                //if( unit.unitType == UnitMeta.UnitType.KING  )
                //    Debug.Log($"{unit.transform.name} {unit.transform.tag} RotateTowards To Target Angle {Quaternion.Angle(transform.rotation, targetRotation)} , AttackAngle: {AttackAgent.AttackAngle()}");
                if (Quaternion.Angle(transform.rotation, targetRotation) < AttackAgent.AttackAngle()) {
                    return true;
                }
                return false;
            }
            /// <summary>
            /// Returns the radius of the agent.
            /// </summary>
            public override float Radius()
            {
                return unit.GetUnitMovement().GetRadius();
            }

            /// <summary>
            /// Starts or stops the rotation from updating.
            /// </summary>
            public override void UpdateRotation(bool update)
            {
                unit.GetUnitMovement().updateRotation(update);
            }

            /// <summary>
            /// Stops the agent from moving.
            /// </summary>
            public override void Stop()
            {
                unit.GetUnitMovement().stop();
            }

            /// <summary>
            /// The task has ended. Perform any cleanup.
            /// </summary>
            public override void End()
            {
                Stop();
                unit.GetUnitMovement().updateRotation(true);
                unit.GetUnitMovement().SetVelocity(Vector3.zero);
            }
        }

        /// <summary>
        /// Adds the agent to the agent list.
        /// </summary>
        /// <param name="agent">The agent to add.</param>
        protected override void AddAgentToGroup(Behavior agent, int index)
        {
            base.AddAgentToGroup(agent, index);

            if (tacticalAgent == null && gameObject == agent.gameObject) {
                tacticalAgent = new NavMeshTacticalAgent(agent.transform);
                //tacticalAgent.AttackOffset = attackOffset.Value;
                //tacticalAgent.TargetOffset = targetOffset.Value;
                // Tactical Agent CanSeeTarget Physics.Linecast, if no offset, Giant cannot hit Loki
                tacticalAgent.AttackOffset = new Vector3 (1f,1f,1f);
                tacticalAgent.TargetOffset = new Vector3(1f, 1f, 1f);
            }
        }
    }
}