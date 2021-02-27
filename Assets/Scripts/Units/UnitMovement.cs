using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class UnitMovement : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private float chaseRange = 10f;
    [SerializeField] public NetworkAnimator unitNetworkAnimator = null;
    [SerializeField] public LineRenderer lineRenderer = null;
    [SerializeField] public GameObject circleMarker = null;
    public float OriginoSpeed;
    private float stoppingDistance = 1f;
    #region Server
    private float startTime = 3;
    private void Start()
    {
        OriginoSpeed = agent.speed;
    }
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
        GameStartCountDown();
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
        if ( !GetComponentInParent<BattleFieldRules>().IsInField(GetComponentInParent<Transform>()) && CompareTag("Player0"))
        {
            if (GetComponentInParent<Unit>().unitType == UnitMeta.UnitType.SPEARMAN)
            {
                GetComponentInParent<UnitPowerUp>().powerUp(GetComponentInParent<Unit>(), 3);
                GetComponentInParent<UnitPowerUp>().RpcPowerUp(GetComponentInParent<Transform>().gameObject, 3);
                Scale(GetComponentInParent<Transform>());
                RpcScale(GetComponentInParent<Transform>());
            }else if(GetComponentInParent<Unit>().unitType == UnitMeta.UnitType.KNIGHT)
            {
                agent.speed = 100;
            }

        }
            position.y = agent.destination.y;
            if (agent.destination != position)
            {
                //Debug.Log($"ServerMove: {agent.destination} /  {position} ");
                agent.SetDestination(position);
            }

        
    }
        private void Scale(Transform tacticalAgent)
        {
            tacticalAgent.transform.localScale = new Vector3(3, 3, 3);
        }
        [ClientRpc]
        private void RpcScale(Transform tacticalAgent)
        {
            Scale(tacticalAgent);
        }
        public void OLDServerMove(Vector3 position)
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
    public void HideLine()
    {
        circleMarker.SetActive(false);
        lineRenderer.enabled = false;
    }
    public void ShowLine()
    {
        if (agent.path.corners.Length < 2) return;

        lineRenderer.enabled = true;
        lineRenderer.sharedMaterial.SetColor("_Color", Color.gray);
        lineRenderer.positionCount = agent.path.corners.Length;
        lineRenderer.SetPositions(agent.path.corners);
        circleMarker.SetActive(true);
        circleMarker.transform.position = agent.destination;
    }
    public NavMeshAgent GetNavMeshAgent()
    {
        return agent;
    }
}
