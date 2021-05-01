using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class GameStartDisplay : MonoBehaviour
{
    [SerializeField] private GameObject gameStartDisplayParent = null;
    [SerializeField] private TMP_Text StartTime = null;
    [SerializeField] private TMP_Text Times = null;
    private float startTime = 3;
    private float Timer = 180;
    private void Start()
    {
        
    }

    private void Update()
    {
        GameStartCountDown();
    }


    private void GameStartCountDown()
    {
        startTime -= 1 * Time.deltaTime;
        
        Timer -= Time.deltaTime;
        
        if (startTime <= 0)
        {

            float minutes = Mathf.FloorToInt(Timer / 60);
            float seconds = Mathf.FloorToInt(Timer % 60);
            if (Timer <= 0) { return; }
            Times.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            startTime = 0;
           
        }
        if (startTime <= 30 && startTime > 0)
        {
            StartTime.text = startTime.ToString("0");
            
            
        }
        else if (startTime <= 0)
        {
            gameStartDisplayParent.SetActive(false);
        }
    }
   
}
