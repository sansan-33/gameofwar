using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime.Tactical;
public class UnitMovement : NetworkBehaviour
{
    [SerializeField] public int maxSpeed = 100;
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] public NetworkAnimator unitNetworkAnimator = null;
    [SerializeField] public GameObject circleMarker = null;
    private Collider other;
    public bool isCollided = false;
    public float originalSpeed;
    public bool IS_STUNNED = false;
    private float stoppingDistance = 1f;
    private RTSPlayer player;
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
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
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
        if (IS_STUNNED) { CmdStop(); }
    }
    [Command]
    public void CmdTest()
    {
        Debug.Log("test");
    }
    [Command]
    public void CmdStun()
    {
        Debug.Log("CmdStun");
          ServerStun();
    }
    [Server]
    public void ServerStun()
    {
        IS_STUNNED = true;
        Debug.Log($"Uniut movement is stuned = {IS_STUNNED}");
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
        isCollided = false;
        ServerMove(position);
    }
    [Server]
    public void ServerMove(Vector3 position)
    {
        position.y = agent.destination.y;
        if (agent.destination != position)
        {
            agent.SetDestination(position);
            agent.isStopped = false;
        }
    }
    [Command]
    public void CmdRotate(Quaternion targetRotation)
    {
        ServerRotate(targetRotation);
    }
    [Server]
    public void ServerRotate(Quaternion targetRotation)
    {
        agent.updateRotation = false;
        agent.transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, agent.angularSpeed * Time.deltaTime);
    }
    [Command]
    public void CmdStop()
    {
        //Debug.Log($"Command stop");
        ServerStop();
    }
    [Server]
    public void ServerStop()
    {
        //Debug.Log($"Server stop");
        if (agent.hasPath)
        {
            //Debug.Log($"Server b4 stop agent {agent.transform.name} , original speed {agent.speed} velocity {agent.velocity} acceleration {agent.acceleration}");
            //Unity Bug, velocity too fast and agent not stop immediately, need to set zero to force it stop
            agent.velocity = new Vector3();
            agent.ResetPath();
            agent.isStopped = true;
            //Debug.Log($"Server stop agent {agent.transform.name} , stooped speed {agent.speed} velocity {agent.velocity} acceleration {agent.acceleration} === stopped");
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
    public bool HasArrived()
    {
        return agent.pathPending && (transform.position - agent.destination).magnitude <= agent.stoppingDistance;
    }
    public bool isCollide()
    {
        Collider[] hitColliders = Physics.OverlapBox(this.transform.GetComponent<Targetable>().GetAimAtPoint().transform.position, transform.localScale * 3, Quaternion.identity, LayerMask.GetMask("Unit"));
        int i = 0;

        //Check when there is a new collider coming into contact with the box
        while (i < hitColliders.Length)
        {
            other = hitColliders[i++];

            if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)
            {
                //Debug.Log($"Attack {targeter} , Hit Collider {hitColliders.Length} , Player Tag {targeter.tag} vs Other Tag {other.tag}");
                //Check for either player0 or king0 collide their team member
                if (other.tag.Contains("" + player.GetPlayerID()) && this.transform.tag.Contains("" + player.GetPlayerID())) { continue; }  //check to see if it belongs to the player, if it does, do nothing
                if (other.tag.Contains("" + player.GetEnemyID()) && this.transform.tag.Contains("" + player.GetEnemyID())) { continue; }  //check to see if it belongs to the player, if it does, do nothing

            }
            else // Multi player seneriao
            {
                //Debug.Log($"Multi player seneriao ");
                if (other.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity))  //try and get the NetworkIdentity component to see if it's a unit/building 
                {
                    if (networkIdentity.hasAuthority) { continue; }  //check to see if it belongs to the player, if it does, do nothing
                }
            }
            //Debug.Log($"Attacker {targeter} --> Enemy {other} tag {other.tag}");
            isCollided = true;
            return true;
        }
        return false;
    }
    public IDamageable collideTarget()
    {
        return other.transform.GetComponent<IDamageable>();
    }
    public Transform collideTargetTransform()
    {
        return other.transform;
    }

}
