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
            private NavMeshAgent navMeshAgent;
            private bool destinationSet;

            /// <summary>
            /// Caches the component references and initialize default values.
            /// </summary>
            public NavMeshTacticalAgent(Transform agent) : base(agent)
            {
                //navMeshAgent = agent.GetComponent<NavMeshAgent>();
                navMeshAgent = agent.GetComponentInParent<Unit>().GetUnitMovement().GetNavMeshAgent();
                if (navMeshAgent.hasPath) {
                    navMeshAgent.ResetPath();
                    navMeshAgent.isStopped = true;
                }
            }

            /// <summary>
            /// Sets the destination.
            /// </summary>
            public override void SetDestination(Vector3 destination)
            {
                destinationSet = true;
                destination.y = navMeshAgent.destination.y;
                navMeshAgent.GetComponentInParent<Unit>().GetUnitPowerUp().cmdPowerUp();
                //Once they are coilled with others. their cannot move again even killed the target because the destination is the same
                if (navMeshAgent.destination != destination || navMeshAgent.GetComponentInParent<Unit>().GetUnitMovement().isCollided) {
                    navMeshAgent.GetComponentInParent<Unit>().GetUnitMovement().CmdTrigger("run");
                    navMeshAgent.GetComponentInParent<Unit>().GetUnitMovement().CmdMove(destination);
                    //navMeshAgent.GetComponentInParent<Unit>().GetUnitMovement().ShowLine();
                    //navMeshAgent.SetDestination(destination);
                    //navMeshAgent.isStopped = false;
                }
            }
            /// <summary>
            /// Has the agent arrived at its destination?
            /// </summary>
            public override bool HasArrived()
            {
                navMeshAgent.GetComponentInParent<Unit>().GetUnitMovement().CmdTrigger("wait");
                return destinationSet && navMeshAgent.GetComponentInParent<Unit>().GetUnitMovement().HasArrived();
            }

            /// <summary>
            /// Rotates towards the target rotation.
            /// </summary>
            public override bool RotateTowards(Quaternion targetRotation)
            {
                if (navMeshAgent.updateRotation) {
                    navMeshAgent.updateRotation = false;
                }
                //transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, navMeshAgent.angularSpeed * Time.deltaTime);
                navMeshAgent.GetComponentInParent<Unit>().GetUnitMovement().CmdRotate(targetRotation);
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
                return navMeshAgent.radius;
            }

            /// <summary>
            /// Starts or stops the rotation from updating.
            /// </summary>
            public override void UpdateRotation(bool update)
            {
                navMeshAgent.updateRotation = update;
            }

            /// <summary>
            /// Stops the agent from moving.
            /// </summary>
            public override void Stop()
            {
                //if (navMeshAgent.hasPath) {
                    //Debug.Log("1 Nav Mesh Tactical Stop --> has path");
                    //navMeshAgent.isStopped = true;
                    destinationSet = false;
                    navMeshAgent.GetComponentInParent<Unit>().GetUnitMovement().CmdStop();
                //}
                //Debug.Log("2 Nav Mesh Tactical Stop again");
                //navMeshAgent.GetComponentInParent<Unit>().GetUnitMovement().CmdStop();
            }

            /// <summary>
            /// The task has ended. Perform any cleanup.
            /// </summary>
            public override void End()
            {
                Stop();
                navMeshAgent.updateRotation = true;
                navMeshAgent.velocity = Vector3.zero;
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
                tacticalAgent.AttackOffset = attackOffset.Value;
                tacticalAgent.TargetOffset = targetOffset.Value;
            }
        }
    }
}