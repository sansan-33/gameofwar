using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitAnimator : NetworkBehaviour
{
    [SerializeField] public NetworkAnimator networkAnim;
    [SerializeField] public Animator anim;
    [SerializeField] public AudioSource audioSource;
    AnimatorClipInfo[] m_CurrentClipInfo;
    [SyncVar] private AnimState currentState;

    public enum AnimState { ATTACK, ATTACK0, ATTACK1, ATTACK2, DEFEND, GETHIT, LOCOMOTION, NOTHING, IDLE , DIE, PROVOKE, VICTORY, OPEN};
    [SyncVar] public bool isAttacking = false;
    private Dictionary<string, float> clipLength =  new Dictionary<string, float>();
    System.Random rand;
    UnitAnimator.AnimState[] ATTACK_RAND = { UnitAnimator.AnimState.ATTACK0, UnitAnimator.AnimState.ATTACK1, UnitAnimator.AnimState.ATTACK2 };

    public override void OnStartServer()
    {
        SetAnimationState();
    }
    public override void OnStartClient()
    {
        SetAnimationState();
    }

    private void SetAnimationState()
    {
        networkAnim = GetComponent<NetworkAnimator>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        rand = new System.Random();
        //Initial state set to prevent attack state delay when checking current state and new state
        currentState = AnimState.IDLE;
        //string weapontype = "_" + UnitMeta.KeyWeaponType[GetComponent<Unit>().unitKey].ToString().ToUpper();
        AnimationClip[] clips = networkAnim.animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name.Contains("ATTACK"))
            {
                if(!clipLength.ContainsKey(clip.name))
                    clipLength.Add(clip.name, clip.length);
            }
        }
    }

    void ChangeAnimationState(AnimState newState)
    {
        //if(name.Contains("TANK"))
        //Debug.Log($"1 {name} {tag} ChangeAnimationState:  currentState: {currentState} / newState: {newState}");
        if (currentState == newState) return;
        //if (currentState.ToString().Contains("ATTACK") && newState.ToString().Contains("ATTACK")) return;
        string animState = newState.ToString();
        ResetAll(animState);
        if (newState == AnimState.ATTACK0 || newState == AnimState.ATTACK1  || newState == AnimState.ATTACK2 || newState == AnimState.PROVOKE) {
            networkAnim.SetTrigger(animState);
            if(audioSource !=null)
                audioSource.PlayDelayed(0.2f);
            return;
        }
        currentState = newState;
        anim.SetBool(animState, true);
    }
    void ResetAll(string animState)
    {
        anim.SetBool("DEFEND", false);
        anim.SetBool("LOCOMOTION", false);
        anim.SetBool("VICTORY", false);
        anim.SetBool(animState, false);
    }
   
    public void HandleStateControl(AnimState newState)
    {
        var n = rand.Next(0, 3);
        if(newState == AnimState.ATTACK)
        {
            newState = ATTACK_RAND[n];
        }
        if (!isAttacking) {
            
            if (newState != AnimState.LOCOMOTION)
            {
                SetFloat("moveSpeed", 0);
                //SetFloat("direction", 0);
            }
            if (newState.ToString().Contains("ATTACK") || newState.ToString().Contains("PROVOKE") || newState.ToString().Contains("VICTORY")) {
                var defaultClipLength = 0f;
                isAttacking = true;
                clipLength.TryGetValue(newState.ToString() , out defaultClipLength);
                SetFloat("animSpeed", defaultClipLength / GetComponent<IAttack>().RepeatAttackDelay());
                Invoke("AttackCompleted", GetComponent<IAttack>().RepeatAttackDelay());
            }
            ChangeAnimationState(newState);
        }
        else {
            //if (name.Contains("TANK"))
            //    Debug.Log($"Unit Anim {name} {tag} blocked , {newState}, isAttacking ? {isAttacking}");
        }
    }
     
    private void AttackCompleted()
    {
        isAttacking = false;
        currentState = AnimState.IDLE;
    }

    public void StateControl(AnimState newState)
    {
        /*
        if (!hasAuthority) {
            if (name.Contains("TANK"))
                Debug.Log($"{name} {tag} no authority {newState}");
            return;  }
        HandleStateControl(newState);
        */
        
        if (isServer)
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
        anim.SetFloat(type, value);
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