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

public class CinemachineShake : NetworkBehaviour {

    //public static CinemachineShake Instance { get; private set; }

    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;
    private float shakeTimer=.05f;
    private float shakeTimerTotal=1f;
    private float startingIntensity=.01f;

    //private void Awake() {
    //    Instance = this;
    //    cinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
    //}

    public void ShakeCamera() {
        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = 
            cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = startingIntensity;

    }

    private void Update() {
        if (shakeTimer > 0) {
            shakeTimer -= Time.deltaTime;
            CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
                cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            
            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 
                Mathf.Lerp(startingIntensity, 0f, 1 - shakeTimer / shakeTimerTotal);

        }
        else
        {
            Destroy(gameObject);
        }
    }

}
