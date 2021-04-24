using Mirror;
using UnityEngine;

public class UnitAnimator : NetworkBehaviour
{
    [SerializeField] public NetworkAnimator networkAnim;
    [SerializeField] public Animator anim;

    /*
    public override void OnStartServer()
    {
        networkAnim = GetComponent<NetworkAnimator>();
        anim = GetComponent<Animator>();
        //networkAnim.animator.SetFloat("MoveSpeed", speed); //will act as multiplier to the speed of the run animation clip
        //networkAnim.animator.SetBool("IsMoving", true);
    }
    */
    public void trigger(string type)
    {
        float animSpeed = 1f; // Original Speed
        networkAnim.animator.speed = animSpeed;
        networkAnim.SetTrigger(type);
        //CmdTrigger(type, animSpeed);
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
        Debug.Log($"111 Unit Animator {tag} {name} set bool {type} {state}" );
        //networkAnim.animator.SetBool(type, state);
        anim.SetBool(type, state);
        //CmdSetBool(type, state);
    }
    [Command]
    public void CmdSetBool(string animationType, bool state)
    {
        Debug.Log($"Unit Animator {tag} {name} CMD set bool {animationType} {state}");
        ServerSetBool(animationType,state);
    }

    [Server]
    public void ServerSetBool(string animationType, bool state)
    {
        Debug.Log($"Unit Animator {tag} {name}  Server set bool {animationType} {state}");
        networkAnim.animator.SetBool(animationType, state);
        anim.SetBool(animationType, state);
    }

}
