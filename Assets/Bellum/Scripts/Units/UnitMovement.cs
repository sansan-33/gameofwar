using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime.Tactical;
public class UnitMovement : NetworkBehaviour , IUnitMovement
{
    [SerializeField] public int maxSpeed = 100;
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] public GameObject circleMarker = null;
    private Collider other;
    public bool isCollided = false;
    public float originalSpeed;
    public bool IS_STUNNED = false;
    private float stoppingDistance = 1f;
    private RTSPlayer player;
    #region Server
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
    }
    [ServerCallback]
    private void Update()
    {
        if (IS_STUNNED) { CmdStop(); }
    }
    public void move(Vector3 position)
    {
        CmdMove(position);
    }
    public void stop()
    {
        CmdStop();
    }
    public void rotate(Quaternion quaternion)
    {
        CmdRotate(quaternion);
    }
    public void updateRotation(bool update)
    {
        agent.updateRotation = update;
    }
    public float GetSpeed(UnitMeta.SpeedType speedType)
    {
        switch (speedType)
        {
            case UnitMeta.SpeedType.MAX:
                return maxSpeed;
            case UnitMeta.SpeedType.CURRENT:
                return agent.speed;
            case UnitMeta.SpeedType.ORIGINAL:
                return originalSpeed;
            default:
                return 0;
        }
    }
    public void SetSpeed(UnitMeta.SpeedType speedType ,float _speed)
    {
        switch (speedType)
        {
            case UnitMeta.SpeedType.MAX:
                maxSpeed = (int) _speed;
                break;
            case UnitMeta.SpeedType.CURRENT:
                agent.speed = _speed;
                break;
            case UnitMeta.SpeedType.ORIGINAL:
                originalSpeed = _speed;
                break;
            default:
                break;
        }
    }
    public Vector3 GetVelocity()
    {
        return agent.velocity;
    }
    public void SetVelocity(Vector3 _velocity)
    {
        agent.velocity = _velocity;
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

    #endregion
   
    public NavMeshAgent GetNavMeshAgent()
    {
        return agent;
    }
    public bool hasArrived()
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
            isCollided = true;
            return true;
        }
        return false;
    }
    public Transform collideTargetTransform()
    {
        return other.transform;
    }
    public float GetRadius()
    {
        return agent.radius;
    }
    public bool collided()
    {
        return isCollided;
    }
    public void provoke(bool provoke)
    {

    }
}
