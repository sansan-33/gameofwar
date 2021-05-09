using System.Collections;
using Mirror;
using UnityEngine;

public class UnitAnimator : NetworkBehaviour
{
    [SerializeField] public NetworkAnimator networkAnim;
    AnimatorClipInfo[] m_CurrentClipInfo;
    [SyncVar] private AnimState currentState;
    public enum AnimState { IDLE, ATTACK, DEFEND, RUN , GETHIT, WALK };
    bool isAttacking = false;
   
    public override void OnStartServer()
    {
        networkAnim = GetComponent<NetworkAnimator>();
    }
    public override void OnStartClient()
    {
        networkAnim = GetComponent<NetworkAnimator>();
    }
    void ChangeAnimationState(AnimState newState, string type)
    {
        if (currentState == newState) return;
        networkAnim.animator.Play(newState.ToString() + type.ToUpper(), -1, 0f);
        currentState = newState;
    }
    public void HandleStateControl(AnimState newState)
    {
        string weapontype = "";
        if (newState == AnimState.ATTACK) {
            if (!isAttacking) {
                isAttacking = true;
                weapontype = "_" + UnitMeta.KeyWeaponType[GetComponent<Unit>().unitKey].ToString().ToLower();
                AnimationClip[] clips = networkAnim.animator.runtimeAnimatorController.animationClips;
                float clipLength = 0f;
                foreach (AnimationClip clip in clips)
                {
                    Debug.Log($"Attack anim {clip.name.ToLower()} {clip.length}");
                    if (clip.name.ToLower() == "attack" + weapontype)
                    {
                        clipLength = clip.length;
                        break;
                    }
                }
                networkAnim.animator.SetFloat("animSpeed", clipLength / GetComponent<IAttack>().RepeatAttackDelay() );
                Invoke("AttackCompleted", clipLength);
            }
        }
        ChangeAnimationState(newState, weapontype);
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