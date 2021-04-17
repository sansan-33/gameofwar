using UnityEngine;
// Note this line, if it is left out, the script won't know that the class 'Path' exists and it will throw compiler errors
// This line should always be present at the top of scripts which use pathfinding
using Pathfinding;
using Mirror;

public class AstarAI : NetworkBehaviour
{
    public Transform targetPosition;

    private Seeker seeker;
    //private CharacterController controller;

    public Path path;

    public float speed = 2;

    public float nextWaypointDistance = 3;

    private int currentWaypoint = 0;

    public bool reachedEndOfPath;

    [SerializeField] public NetworkAnimator unitNetworkAnimator = null;
    bool IS_STUNNED = false;

    public void Start()
    {
        seeker = GetComponent<Seeker>();
        // If you are writing a 2D game you should remove this line
        // and use the alternative way to move sugggested further below.
        //controller = GetComponent<CharacterController>();

        // Start a new path to the targetPosition, call the the OnPathComplete function
        // when the path has been calculated (which may take a few frames depending on the complexity)
        //seeker.StartPath(transform.position, targetPosition.position, OnPathComplete);
    }
    [Command]
    public void CmdMove(Vector3 position)
    {
        ServerMove(position);
    }
    [Server]
    public void ServerMove(Vector3 position)
    {
        position.y = transform.position.y;
        seeker.StartPath(transform.position, targetPosition.position, OnPathComplete);
    }
    public void OnPathComplete(Path p)
    {
        Debug.Log("A path was calculated. Did it fail with an error? " + p.error);

        if (!p.error)
        {
            path = p;
            // Reset the waypoint counter so that we start to move towards the first point in the path
            currentWaypoint = 0;
        }
    }
    [ServerCallback]
    public void Update()
    {
        if (path == null)
        {
            // We have no path to follow yet, so don't do anything
            return;
        }

        // Check in a loop if we are close enough to the current waypoint to switch to the next one.
        // We do this in a loop because many waypoints might be close to each other and we may reach
        // several of them in the same frame.
        reachedEndOfPath = false;
        // The distance to the next waypoint in the path
        float distanceToWaypoint;
        while (true)
        {
            // If you want maximum performance you can check the squared distance instead to get rid of a
            // square root calculation. But that is outside the scope of this tutorial.
            distanceToWaypoint = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]);
            if (distanceToWaypoint < nextWaypointDistance)
            {
                // Check if there is another waypoint or if we have reached the end of the path
                if (currentWaypoint + 1 < path.vectorPath.Count)
                {
                    currentWaypoint++;
                }
                else
                {
                    // Set a status variable to indicate that the agent has reached the end of the path.
                    // You can use this to trigger some special code if your game requires that.
                    reachedEndOfPath = true;
                    break;
                }
            }
            else
            {
                break;
            }
        }

        // Slow down smoothly upon approaching the end of the path
        // This value will smoothly go from 1 to 0 as the agent approaches the last waypoint in the path.
        var speedFactor = reachedEndOfPath ? Mathf.Sqrt(distanceToWaypoint / nextWaypointDistance) : 1f;

        // Direction to the next waypoint
        // Normalize it so that it has a length of 1 world unit
        Vector3 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
        // Multiply the direction by our desired speed to get a velocity
        Vector3 velocity = dir * speed * speedFactor;

        // Move the agent using the CharacterController component
        // Note that SimpleMove takes a velocity in meters/second, so we should not multiply by Time.deltaTime
        //controller.SimpleMove(velocity);

        // If you are writing a 2D game you should remove the CharacterController code above and instead move the transform directly by uncommenting the next line
        transform.position += velocity * Time.deltaTime;
        if (IS_STUNNED) { CmdStop(); }
    }

    public void HandleDieAnnimation()
    {
        CmdTrigger("die");
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
    public void CmdRotate(Quaternion targetRotation)
    {
        ServerRotate(targetRotation);
    }
    [Server]
    public void ServerRotate(Quaternion targetRotation)
    {
        //agent.updateRotation = false;
        //agent.transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, agent.angularSpeed * Time.deltaTime);
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
        
    }
    [Server]
    private void ServerHandleGameOver()
    {
        
    }
    private void GameStartCountDown()
    {
        
    }
    public bool HasArrived()
    {
        return false;
    }
    public bool isCollide()
    {
        return false;
    }
    /*
    public NavMeshAgent GetNavMeshAgent()
    {
        return agent;
    }
    public IDamageable collideTarget()
    {
        return other.transform.GetComponent<IDamageable>();
    }
    public Transform collideTargetTransform()
    {
        return other.transform;
    }
    */



}