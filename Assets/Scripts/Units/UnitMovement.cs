using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class UnitMovement : NetworkBehaviour
{
    [SerializeField] public int maxSpeed = 100;
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] public NetworkAnimator unitNetworkAnimator = null;
    [SerializeField] public GameObject circleMarker = null;
    public float originalSpeed;
    private float stoppingDistance = 1f;
    #region Server
    private float startTime = 3;
    private void Start()
    {
        originalSpeed = agent.speed;
    }
    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
        
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }
    public override void OnStartClient()
    {
        GameStartCountDown();
    }
    [ServerCallback]
    private void Update()
    {
        /*
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
        */
    }
    [Command]
    public void CmdTrigger(string animationType)
    {
        ServerTrigger(animationType);
    }

    [Server]
    public void ServerTrigger(string animationType)
    {
        unitNetworkAnimator.SetTrigger(animationType);
    }
    

    [Command]
    public void CmdMove(Vector3 position)
    {
        ServerMove(position);
    }
    [Server]
    public void ServerMove(Vector3 position)
    {
        position.y = agent.destination.y;
        if (agent.destination != position)
        {
            agent.SetDestination(position);
        }
    }

    [Server]
    private void ServerHandleGameOver()
    {
        agent.ResetPath();
    }
    private void GameStartCountDown()
    {
        startTime -= 1 * Time.deltaTime;
        if (startTime <= 0)
        {
            startTime = 0;
        }
        if (startTime <= 3 && startTime > 0)
        {

            agent.ResetPath();

        }
        
    }

    #endregion
   
    public NavMeshAgent GetNavMeshAgent()
    {
        return agent;
    }
}
