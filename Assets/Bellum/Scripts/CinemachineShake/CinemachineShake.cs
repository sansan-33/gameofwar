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
    [HideInInspector] public float shakeTime;
    private float shakeTimer=.05f;
    private float secondShakeTimer=.05f;
    private float shakeTimerTotal=1f;
    private float startingIntensity=100.01f;

    //private void Awake() {
    //    Instance = this;
    //    cinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
    //}

    public void ShakeCamera(float _shakeTime) {
        this.shakeTime = _shakeTime;
        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = 
            cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = startingIntensity;

    }

    private void Update()
    {   if(shakeTime > 0)
        {
            Debug.Log($"shake time {shakeTime},{shakeTimer}");
            shakeTime -= Time.deltaTime;
            if (shakeTimer > 0)
            {
            //Debug.Log("shake");
                secondShakeTimer = .05f;
                shakeTimer -= Time.deltaTime;
                GetComponent<CinemachineVirtualCamera>().enabled = true;
                CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
                    cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

                cinemachineBasicMultiChannelPerlin.m_AmplitudeGain =
                    Mathf.Lerp(startingIntensity, 0f, 1 - shakeTimer / shakeTimerTotal);
                
            }
            else
             {
            //Debug.Log("return shake !!!!!!!!!!!");
            GetComponent<CinemachineVirtualCamera>().enabled = false;
                if (secondShakeTimer > 0)
                {
                    // Debug.Log($"count shake {secondShakeTimer}");
                    secondShakeTimer -= Time.deltaTime;
                }
                else { shakeTimer = .05f; 
            //Debug.Log("re shake");
            }
            }
        }
        else { Destroy(gameObject); }
        
    }

}
