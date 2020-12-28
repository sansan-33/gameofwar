using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class GameWinDisplay : MonoBehaviour
{
    [SerializeField] private GameObject gameWinDisplayParent = null;
    [SerializeField] private TMP_Text winnerNameText = null;

    private void Start()
    {
        GameWinHandler.ClientOnGameWin += ClientHandleGameWin;
    }

    private void OnDestroy()
    {
        GameWinHandler.ClientOnGameWin -= ClientHandleGameWin;
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

    private void ClientHandleGameWin(string winner)
    {
        winnerNameText.text = $"{winner} Has Won!";

        gameWinDisplayParent.SetActive(true);
    }
}
