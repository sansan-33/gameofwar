
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Mirror;

public class CMFreeLook : NetworkBehaviour
{


    [SerializeField] private CinemachineFreeLook cinemachineFreeLook;
    Cinemachine.CinemachineImpulseSource source;
    private float shakeTimer = 3000f;
    private float shakeTimerTotal = .1f;
    private float startingIntensity = 15f;


    public void ThirdCamera(GameObject main, GameObject enemy)
    {

        Transform tFollowTarget = main.transform;
        Transform tLookAtTarget = enemy.transform;

        cinemachineFreeLook.LookAt = tLookAtTarget;
        cinemachineFreeLook.Follow = tFollowTarget;
    }
    private void Update()
    {
        /*
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
        }else{
             Destroy(gameObject);
        }
        */
    }

}