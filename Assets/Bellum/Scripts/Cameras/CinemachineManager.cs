/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Mirror;

public class CinemachineManager : NetworkBehaviour {

    public static CinemachineManager Instance { get; private set; }

    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;
    [SerializeField] private CinemachineFreeLook cinemachineFreeLook;

    [HideInInspector] public float shakeTime;
    private float shakeTimer=.05f;
    private float secondShakeTimer=.05f;
    private float shakeTimerTotal=1f;
    private float startingIntensity=0.001f;
   
    public override void OnStartServer()
    {
        Instance = this;
        cinemachineVirtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
        cinemachineFreeLook = GetComponentInChildren<CinemachineFreeLook>();
    }
    public override void OnStartClient()
    {
        Instance = this;
        cinemachineVirtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
        cinemachineFreeLook = GetComponentInChildren<CinemachineFreeLook>();
    }
    //==================================== Set Skill For Unit
    public void shake()
    {
        if (isServer)
            RpcShake();
        else
            CmdShake();
    }
    [Command(requiresAuthority = false)]
    public void CmdShake( )
    {
        ServerShake();
    }
    [ClientRpc]
    public void RpcShake()
    {
        HandleShake();
    }
    [Server]
    public void ServerShake()
    {
        HandleShake();
    }
    private void HandleShake()
    {
        cinemachineVirtualCamera.enabled = true;
        ShakeCamera(0.05f);
    }
    public void ShakeCamera(float _shakeTime) {
        this.shakeTime = _shakeTime;
        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = startingIntensity;
    }
    public void ThirdCamera(GameObject main, GameObject enemy)
    {
        if (isServer)
            RpcThirdCamera(main, enemy);
        else
            CmdThirdCamera(main, enemy);
    }
    [Command(requiresAuthority = false)]
    public void CmdThirdCamera(GameObject main, GameObject enemy)
    {
        ServerThirdCamera(main, enemy);
    }
    [ClientRpc]
    public void RpcThirdCamera(GameObject main, GameObject enemy)
    {
        HandleThirdCamera(main, enemy);
    }
    [Server]
    public void ServerThirdCamera(GameObject main, GameObject enemy)
    {
        HandleThirdCamera(main, enemy);
    }
    public void HandleThirdCamera(GameObject main, GameObject enemy)
    {
        if (main == null || enemy == null) { return; }
        Transform tFollowTarget = main.transform;
        Transform tLookAtTarget = enemy.transform;
        cinemachineFreeLook.enabled = true;
        cinemachineFreeLook.LookAt = tLookAtTarget;
        cinemachineFreeLook.Follow = tFollowTarget;

    }
    private void Update()
    {   if(shakeTime > 0)
        {
            shakeTime -= Time.deltaTime;
            if (shakeTimer > 0)
            {
            //Debug.Log("shake");
                secondShakeTimer = .05f;
                shakeTimer -= Time.deltaTime;
                cinemachineVirtualCamera.enabled = true;
                CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
                    cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

                cinemachineBasicMultiChannelPerlin.m_AmplitudeGain =
                    Mathf.Lerp(startingIntensity, 0f, 1 - shakeTimer / shakeTimerTotal);
                
            }
            else
             {
                //Debug.Log("return shake !!!!!!!!!!!");
                /*cinemachineVirtualCamera.enabled = false;
                if (secondShakeTimer > 0)
                {
                    // Debug.Log($"count shake {secondShakeTimer}");
                    secondShakeTimer -= Time.deltaTime;
                }
                else { shakeTimer = .05f; */
                { 
                //Debug.Log("re shake");
                }
            }
        }
        else {
            cinemachineVirtualCamera.enabled = false;
            //Destroy(gameObject);
        }

    }

}
