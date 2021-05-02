using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class GameStartDisplay : NetworkBehaviour
{
    [SerializeField] private GameObject gameStartDisplayParent = null;
    [SerializeField] private TMP_Text StartTime = null;
    [SerializeField] private TMP_Text Times = null;
    [SyncVar(hook = "StartTimeing")]
    private float startTime = 3;
    [SyncVar(hook = "Timeing")]
    private float Timer = 180;
    private void Start()
    {
        
    }

    private void Update()
    {
        GameStartCountDown();
        GameEndCountDown();
    }

    [Server]
    private void GameStartCountDown()
    {
        startTime -= 1 * Time.deltaTime;
    }
    [Server]
    private void GameEndCountDown()
    {
        Timer -= Time.deltaTime;
    }
    public void StartTimeing(float oldTime, float newTime)
    {
       // Debug.Log($"oldTime:{oldTime}newTime:{newTime}");
        if (newTime <= 30 && newTime > 0)
        {
            StartTime.text = newTime.ToString("0");
        }
        else if (newTime <= 0)
        {
            gameStartDisplayParent.SetActive(false);
        }
    }
    public void Timeing(float oldTime, float newTime)
    {
        //Debug.Log($"oldTime:{oldTime}newTime:{newTime}");
            float minutes = Mathf.FloorToInt(newTime / 60);
            float seconds = Mathf.FloorToInt(newTime % 60);
            if (newTime <= 0) { return; }
            Times.text = string.Format("{0:00}:{1:00}", minutes, seconds);
           

        
        
    }

}
