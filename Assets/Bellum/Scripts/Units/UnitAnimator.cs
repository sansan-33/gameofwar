using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class UnitAnimator : NetworkBehaviour
{
    [SerializeField] public NetworkAnimator networkAnim;
    AnimatorClipInfo[] m_CurrentClipInfo;
    [SyncVar] private AnimState currentState;
    public enum AnimState { ATTACK, DEFEND, GETHIT, LOCOMOTION, NOTHING, IDLE , DIE};
    bool isAttacking = false;
    private float clipLength = 0f;
    private string attackState = "";
    private string locomotionState = "";

    public override void OnStartServer()
    {
        networkAnim = GetComponent<NetworkAnimator>();
        SetAnimationState();
    }
    public override void OnStartClient()
    {
        networkAnim = GetComponent<NetworkAnimator>();
        SetAnimationState();
    }

    private void SetAnimationState()
    {
        //Initial state set to prevent attack state delay when checking current state and new state
        currentState = AnimState.IDLE;
        string weapontype = "_" + UnitMeta.KeyWeaponType[GetComponent<Unit>().unitKey].ToString().ToUpper();
        AnimationClip[] clips = networkAnim.animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            //Debug.Log($"Attack anim {clip.name} {clip.length}");
            if (clip.name  == "ATTACK" + weapontype)
            {
                clipLength = clip.length;
                attackState = clip.name;
                break;
            }
        }
        locomotionState = "LOCOMOTION";
        if (UnitMeta.UnitKeyRider.TryGetValue(GetComponent<Unit>().unitKey, out bool isRider))
            locomotionState = "WALK_RIDER";  
    }

    void ChangeAnimationState(AnimState newState)
    {
        if (currentState == newState) return;
        string animState = newState.ToString();
        ResetAll(animState);
        if (newState == AnimState.ATTACK) {
            animState = attackState;
            networkAnim.SetTrigger(animState);
            return;
        }
        if (newState == AnimState.LOCOMOTION) {
            animState = locomotionState;
        }
        networkAnim.animator.SetBool(animState, true);
        currentState = newState;
    }
    void ResetAll(string animState)
    {
        networkAnim.animator.SetBool("DEFEND", false);
        networkAnim.animator.SetBool(locomotionState, false);
        networkAnim.animator.SetBool(animState, false);
    }
   
    public void HandleStateControl(AnimState newState)
    {
        if (!isAttacking) {
            //if (newState != AnimState.LOCOMOTION)
            //{
            //    SetFloat("moveSpeed", 0);
            //    SetFloat("direction", 0);
            //}
            if (newState == AnimState.ATTACK) {
                isAttacking = true;
                networkAnim.animator.SetFloat("animSpeed", clipLength / GetComponent<IAttack>().RepeatAttackDelay());
                Invoke("AttackCompleted", GetComponent<IAttack>().RepeatAttackDelay());
                if(GetComponent<Unit>().unitType == UnitMeta.UnitType.ARCHER )
                    Debug.Log($"Unit Anim {name} isAttacking , wait for {GetComponent<IAttack>().RepeatAttackDelay()} sec");
            }
            ChangeAnimationState(newState);
        }
        else {
            //Debug.Log($"Unit Anim {name} blocked , {newState}");
        }
    }
     
    private void AttackCompleted()
    {
        isAttacking = false;
        currentState = AnimState.IDLE;
    }

    public void StateControl(AnimState newState)
    {
        if(isServer)
            RpcStateControl(newState);
        else
            CmdStateControl(newState);
    }
   
    [Command]
    public void CmdStateControl(AnimState newState)
    {
        ServerStateControl(newState);
    }
    [ClientRpc]
    public void RpcStateControl(AnimState newState)
    {
        HandleStateControl(newState);
    }
    [Server]
    public void ServerStateControl(AnimState newState)
    {
        HandleStateControl(newState);
    }
    public void SetFloat(string type , float value)
    {
        networkAnim.animator.SetFloat(type, value);
    }
}


/*

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [SerializeField]
    private float walkSpeed = 5f;

    private Animator animator;

    private float xAxis;
    private float yAxis;
    private Rigidbody2D rb2d;
    private bool isJumpPressed;
    private float jumpForce = 850;
    private int groundMask;
    private bool isGrounded;
    private string currentAnimaton;
    private bool isAttackPressed;
    private bool isAttacking;

    [SerializeField]
    private float attackDelay = 0.3f;

    //Animation States
    const string PLAYER_IDLE = "Player_idle";
    const string PLAYER_RUN = "Player_run";
    const string PLAYER_JUMP = "Player_jump";
    const string PLAYER_ATTACK = "Player_attack";
    const string PLAYER_AIR_ATTACK = "Player_air_attack";

    //=====================================================
    // Start is called before the first frame update
    //=====================================================
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        groundMask = 1 << LayerMask.NameToLayer("Ground");

    }

    //=====================================================
    // Update is called once per frame
    //=====================================================
    void Update()
    {
        //Checking for inputs
        xAxis = Input.GetAxisRaw("Horizontal");

        //space jump key pressed?
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isJumpPressed = true;
        }

        //space Atatck key pressed?
        if (Input.GetKeyDown(KeyCode.RightControl))
        {
            isAttackPressed = true;
        }
    }

    //=====================================================
    // Physics based time step loop
    //=====================================================
    private void FixedUpdate()
    {
        //check if player is on the ground
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, groundMask);

        if (hit.collider != null)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        //------------------------------------------

        //Check update movement based on input
        Vector2 vel = new Vector2(0, rb2d.velocity.y);

        if (xAxis < 0)
        {
            vel.x = -walkSpeed;
            transform.localScale = new Vector2(-1, 1);
        }
        else if (xAxis > 0)
        {
            vel.x = walkSpeed;
            transform.localScale = new Vector2(1, 1);

        }
        else
        {
            vel.x = 0;

        }

        if (isGrounded && !isAttacking)
        {
            if (xAxis != 0)
            {
                ChangeAnimationState(PLAYER_RUN);
            }
            else
            {
                ChangeAnimationState(PLAYER_IDLE);
            }
        }

        //------------------------------------------

        //Check if trying to jump 
        if (isJumpPressed && isGrounded)
        {
            rb2d.AddForce(new Vector2(0, jumpForce));
            isJumpPressed = false;
            ChangeAnimationState(PLAYER_JUMP);
        }

        //assign the new velocity to the rigidbody
        rb2d.velocity = vel;


        //attack
        if (isAttackPressed)
        {
            isAttackPressed = false;

            if (!isAttacking)
            {
                isAttacking = true;

                if (isGrounded)
                {
                    ChangeAnimationState(PLAYER_ATTACK);
                }
                else
                {
                    ChangeAnimationState(PLAYER_AIR_ATTACK);
                }


                Invoke("AttackComplete", attackDelay);


            }


        }

    }

    void AttackComplete()
    {
        isAttacking = false;
    }

    //=====================================================
    // mini animation manager
    //=====================================================
    void ChangeAnimationState(string newAnimation)
    {
        if (currentAnimaton == newAnimation) return;

        animator.Play(newAnimation);
        currentAnimaton = newAnimation;
    }

}
*/