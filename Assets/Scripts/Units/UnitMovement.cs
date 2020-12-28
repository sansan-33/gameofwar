using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class UnitMovement : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private float chaseRange = 10f;
    [SerializeField] public NetworkAnimator unitNetworkAnimator = null;

    private float stoppingDistance = 1f;
    #region Server

    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    [ServerCallback]
    private void Update()
    {

        Targetable target = targeter.GetTarget();
        
        if (target != null)
        {
            if (agent.remainingDistance < getStoppingDistance()) {
                //Debug.Log($"Reset Path agent.remainingDistance {agent.remainingDistance}  > getStoppingDistance() {getStoppingDistance()} ");
                agent.ResetPath();
                return;
            }

            if ((target.transform.position - transform.position).sqrMagnitude > chaseRange * chaseRange)
            {
                agent.SetDestination(target.transform.position);
            }
            else if(agent.hasPath)
            {
                agent.ResetPath();
            }

            return;
        }

        if (!agent.hasPath) { return; }
        //This is for moving to destination, not include any target.
        //Debug.Log($"Unit Movement agent.remainingDistance : {agent.remainingDistance} / getStoppingDistance() : {getStoppingDistance()}");
        if (agent.remainingDistance > getStoppingDistance()) { return; }

        agent.ResetPath();
    }

    [Command]
    public void CmdMove(Vector3 position)
    {
        ServerMove(position);
    }
    
    [Server]
    public void ServerMove(Vector3 position)
    {
        targeter.ClearTarget();

        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) { return; }

        agent.SetDestination(hit.position);
    }

    [Server]
    private void ServerHandleGameOver()
    {
        agent.ResetPath();
    }

    [Command]
    public void CmdMoveAttack(Vector3 position,  int speed)
    {
        ServerMoveAttack( position, speed);
    }
    [Server]
    public void ServerMoveAttack(Vector3 position, int speed)
    {

        targeter.ClearTarget();

        //if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) { return; }
        //Debug.Log($"1 ServerMoveAttack position {position} / rotation {agent.transform.rotation}");

        agent.speed = speed;
        agent.SetDestination(position);
        
        //Debug.Log($"2 ServerMoveAttack position {position} / rotation {agent.transform.rotation}");
    }
   
    #endregion
    private float getStoppingDistance()
    {
        stoppingDistance = agent.stoppingDistance;
        //Debug.Log($"targeter.targeterAttackType {targeter.targeterAttackType}");
        if (targeter.targeterAttackType == Targeter.AttackType.Shoot)
        {
            stoppingDistance = 60f;
        }
        else
        {
            //stoppingDistance = agent.stoppingDistance;
            stoppingDistance = 1f;
        }

        return stoppingDistance;
    }
}
