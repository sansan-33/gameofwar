using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class GameOverDisplay : MonoBehaviour
{
    [SerializeField] private GameObject gameOverDisplayParent = null;
    [SerializeField] private TMP_Text winnerNameText = null;
    private float Timer = 183;
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

        gameOverDisplayParent.SetActive(true);
    }
    private void ClientHandleGameOverdraw()
    {
        winnerNameText.text = $"Draw!";

        gameOverDisplayParent.SetActive(true);
    }
    public void ClientHandleGameOverResign()
    {
        Debug.Log(1);
        winnerNameText.text = $"Someone give up";

        gameOverDisplayParent.SetActive(true);
    }
}
