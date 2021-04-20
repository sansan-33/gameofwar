using Mirror;
using UnityEngine;

public class UnitAnimator : NetworkBehaviour
{
    public NetworkAnimator networkAnim;

    void Awake()
    {
        networkAnim = GetComponent<NetworkAnimator>();
        //networkAnim.animator.SetFloat("MoveSpeed", speed); //will act as multiplier to the speed of the run animation clip
        //networkAnim.animator.SetBool("IsMoving", true);
    }
    public void trigger(string type)
    {
        float animSpeed = 1f; // Original Speed
        CmdTrigger(type, animSpeed);
    }
    public void trigger(string type, float animSpeed)
    {
        CmdTrigger(type, animSpeed);
    }
    [Command]
    public void CmdTrigger(string animationType, float animSpeed)
    {
        ServerTrigger(animationType, animSpeed);
    }

    [Server]
    public void ServerTrigger(string animationType, float animSpeed)
    {
        networkAnim.animator.speed = animSpeed;
        networkAnim.SetTrigger(animationType);
    }
    public void SetBool(string type, bool state)
    {
        CmdSetBool(type, state);
    }
    [Command]
    public void CmdSetBool(string animationType, bool state)
    {
        ServerSetBool(animationType,state);
    }

    [Server]
    public void ServerSetBool(string animationType, bool state)
    {
        networkAnim.animator.SetBool(animationType, state);
    }

}
