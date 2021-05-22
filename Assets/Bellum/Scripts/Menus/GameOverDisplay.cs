using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverDisplay : MonoBehaviour
{
    [SerializeField] public Canvas gameOverDisplayParent;
    [SerializeField] private TMP_Text winnerNameText = null;
    [SerializeField] private GameObject camFreeLookPrefab = null;
    [SerializeField] private Canvas cardDisplay = null;

    private float Timer = 180;
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
            SceneManager.LoadScene("Scene_Main_Menu");
            //NetworkManager.singleton.offlineScene = "Scene_Main_Menu";
            NetworkManager.singleton.StopClient();
        }
    }

    private void ClientHandleGameOver(string winner)
    {
        GameObject winnerObject = GameObject.FindGameObjectWithTag("King" + (winner.ToLower() == "blue" ? "0" : "1"));
        winnerNameText.text = $"{winner} Team";
        gameOverDisplayParent.enabled = true;
        GameObject cam = Instantiate(camFreeLookPrefab, new Vector3(0, 0, 0), Quaternion.Euler(new Vector3(0, 0, 0)));
        cam.GetComponent<CMFreeLook>().ThirdCamera(winnerObject, winnerObject);
        winnerNameText.DOFade(0f, 0.1f);
        winnerNameText.DOFade(1f, 5f);
        winnerNameText.transform.DOMove(winnerNameText.transform.position + 2 * (Vector3.down), 1.75f).OnComplete(() => {
            //Destroy(transform.root.gameObject);
        });
        winnerNameText.color = winner.ToLower() == "blue" ? Color.blue : Color.red;
        cardDisplay.enabled = false;
    }
    private void ClientHandleGameOverdraw()
    {
        winnerNameText.text = $"Draw!";
        gameOverDisplayParent.enabled = true;
    }
    public void ClientHandleGameOverResign()
    {
        if (gameOverDisplayParent == null) { gameOverDisplayParent = GetComponentInChildren<Canvas>(); }
        winnerNameText.text = $"Someone give up";
        gameOverDisplayParent.enabled = true;
    }
}
