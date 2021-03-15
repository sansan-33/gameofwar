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
