using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class GameStartDisplay : MonoBehaviour
{
    [SerializeField] private GameObject gameStartDisplayParent = null;
    [SerializeField] private TMP_Text StartTime = null;
    private float startTime = 3;
    private void Start()
    {
        
    }

    private void Update()
    {
        GameStartCountDown();
    }


    private void GameStartCountDown()
    {
        startTime -= 1*Time.deltaTime;
        if (startTime <= 0)
        {
            startTime = 0;
        }
        if (startTime <= 3 && startTime > 0)
        {
            StartTime.text = startTime.ToString("0");
            
            
        }
        else if (startTime <= 0)
        {
            gameStartDisplayParent.SetActive(false);
        }
    }
}
