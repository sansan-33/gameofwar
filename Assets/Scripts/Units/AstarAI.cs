using UnityEngine;
// Note this line, if it is left out, the script won't know that the class 'Path' exists and it will throw compiler errors
// This line should always be present at the top of scripts which use pathfinding
using Pathfinding;
using Mirror;

public class AstarAI : NetworkBehaviour, IUnitMovement
{
    //private Seeker seeker;
    private AIPath ai;
    public Path path;

    [SerializeField] public int maxSpeed = 100;

    public float nextWaypointDistance = 3;

    private int currentWaypoint = 0;

    public bool reachedEndOfPath;

    bool IS_STUNNED = false;
    private RTSPlayer player;
    private Collider other;
    public bool isCollided = false;

    public float repathRate = 0.5f;
    private float lastRepath = float.NegativeInfinity;

    public void Start()
    {
        //seeker = GetComponent<Seeker>();
        ai = GetComponent<AIPath>();
    }
    public override void OnStartClient()
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
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
        position.y = 0;
        if (ai.canMove && ai.destination == position) {
            //if (gameObject.name.ToLower().Contains("tank"))
            //    Debug.Log($"same destination {ai.destination} target {position} , save memory not start path");
            return;
        }
        ai.canMove = true;
        //if (Time.time > lastRepath + repathRate && seeker.IsDone())
        //{
        //    lastRepath = Time.time;

        // Start a new path to the targetPosition, call the the OnPathComplete function
        // when the path has been calculated (which may take a few frames depending on the complexity)
        //if (gameObject.name.ToLower().Contains("tank"))
        //        Debug.Log($"ServerMove : {gameObject.name} move from {transform.position} to target {position} /  {ai.destination}, save memory not start path");
            ai.destination = position;
            ai.SearchPath();
            //seeker.StartPath(transform.position, position, OnPathComplete);
        //}
    }
    public void OnPathComplete(Path p)
    {
        // Debug.Log("A path was calculated. Did it fail with an error? " + p.error);

        // Path pooling. To avoid unnecessary allocations paths are reference counted.
        // Calling Claim will increase the reference count by 1 and Release will reduce
        // it by one, when it reaches zero the path will be pooled and then it may be used
        // by other scripts. The ABPath.Construct and Seeker.StartPath methods will
        // take a path from the pool if possible. See also the documentation page about path pooling.
        p.Claim(this);
        if (!p.error)
        {
            if (path != null) path.Release(this);
            path = p;
            // Reset the waypoint counter so that we start to move towards the first point in the path
            currentWaypoint = 0;
        }
        else
        {
            p.Release(this);
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
        Vector3 velocity = dir * ai.maxSpeed / 2 * speedFactor;

        // Move the agent using the CharacterController component
        // Note that SimpleMove takes a velocity in meters/second, so we should not multiply by Time.deltaTime
        //controller.SimpleMove(velocity);

        // If you are writing a 2D game you may want to remove the CharacterController and instead modify the position directly
        transform.position += velocity * Time.deltaTime;
        
        if (IS_STUNNED) { CmdStop(); }
    }

    [Command]
    public void CmdRotate(Quaternion targetRotation)
    {
        ServerRotate(targetRotation);
    }
    [Server]
    public void ServerRotate(Quaternion targetRotation)
    {
        //ai.updateRotation = false;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, ai.rotationSpeed * Time.deltaTime);
    }
    [Command]
    public void CmdStop()
    {
        ServerStop();
    }
    [Server]
    public void ServerStop()
    {
        //ai.isStopped = true;
        ai.canMove = false;
    }
    [Server]
    private void ServerHandleGameOver()
    {
        
    }
    public bool isCollide()
    {
        //Debug.Log($"AstarAI is collide ?  {isCollided}");
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

    public void move(Vector3 position)
    {
        CmdMove(position);
    }

    public void stop()
    {
        CmdStop();
    }

    public void rotate(Quaternion targetRotation)
    {
        CmdRotate(targetRotation);
    }

    public void updateRotation(bool update)
    {
        //ai.updateRotation = update;
    }

    public bool hasArrived()
    {
        return ai.reachedDestination;
    }

    public Transform collideTargetTransform()
    {
        return other.transform;
    }

    public float GetSpeed(UnitMeta.SpeedType speedType)
    {
        if (ai == null) { ai = GetComponent<AIPath>(); }
        switch (speedType)
        {
            case UnitMeta.SpeedType.MAX:
                return this.maxSpeed;
            case UnitMeta.SpeedType.CURRENT:
                return ai.maxSpeed;
            default:
                return 0;
        }
    }

    public void SetSpeed(UnitMeta.SpeedType speedType, float _speed)
    {
        if (ai == null) { ai = GetComponent<AIPath>(); }
        switch (speedType)
        {
            case UnitMeta.SpeedType.MAX:
                this.maxSpeed = (int)_speed;
                break;
            case UnitMeta.SpeedType.CURRENT:
                ai.maxSpeed = _speed;
                break;
            default:
                break;
        }
    }

    public Vector3 GetVelocity()
    {
        return ai.velocity;
    }

    public void SetVelocity(Vector3 velocity)
    {
    }
    public float GetRadius()
    {
        return 0f;
    }
    public bool collided()
    {
        return isCollided;
    }

}