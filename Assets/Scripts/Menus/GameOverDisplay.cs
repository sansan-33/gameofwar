using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class GameOverDisplay : MonoBehaviour
{
    [SerializeField] public Canvas gameOverDisplayParent;
    [SerializeField] private TMP_Text winnerNameText = null;
    private float Timer = 1830;
    private void Update()
    {
        Timer -= Time.deltaTime;
        if(Timer <= 0)
        {
            ClientHandleGameOverdraw();
        }
    }
    private void Start()
    {
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    public void LeaveGame()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();
        }
    }

    private void ClientHandleGameOver(string winner)
    {
        winnerNameText.text = $"{winner} Has Won!";
        gameOverDisplayParent.enabled = true;
    }
    private void ClientHandleGameOverdraw()
    {
        winnerNameText.text = $"Draw!";
        gameOverDisplayParent.enabled = true;
    }
    public void ClientHandleGameOverResign()
    {
        
        if (gameOverDisplayParent == null) { gameOverDisplayParent = GetComponentInChildren<Canvas>(); }
        Debug.Log($"ClientHandleGameOverResign {gameOverDisplayParent.name}");
        winnerNameText.text = $"Someone give up";
        gameOverDisplayParent.enabled = true;
    }
}
