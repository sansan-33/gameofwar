using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Mirror;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverDisplay : MonoBehaviour
{
    [SerializeField] public Canvas gameOverDisplayParent;
    [SerializeField] private TMP_Text winnerNameText = null;
    [SerializeField] private TMP_Text stat1Text = null;
    [SerializeField] private TMP_Text stat2Text = null;
    [SerializeField] private TMP_Text stat3Text = null;
    [SerializeField] private TMP_Text stat4Text = null;
    [SerializeField] private TMP_Text stat5Text = null;
    [SerializeField] private TMP_Text totalText = null;

    [SerializeField] private GameObject camFreeLookPrefab = null;
    [SerializeField] private Canvas cardDisplay = null;
    [SerializeField] private TMP_Text crownBlueText = null;
    [SerializeField] private TMP_Text crownRedText = null;
    public TacticalBehavior tacticalBehavior;

    private float Timer = 180;
    APIManager apiManager;
    private void Update()
    {
        Timer -= Time.deltaTime;
        if(Timer <= 0) {
            int blueCrown = Int32.Parse(crownBlueText.text);
            int redCrown = Int32.Parse(crownRedText.text);
            if (blueCrown != redCrown && (blueCrown + redCrown) > 0 ) {
                ClientHandleGameOver(blueCrown > redCrown ? "blue" : "red");
            } else
                ClientHandleGameOverdraw();
        }
    }
    private void Start()
    {
        apiManager = new APIManager();
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
        int crownCount = winner.ToLower() == "blue" ? Int32.Parse(crownBlueText.text) : Int32.Parse(crownRedText.text);

        cardDisplay.enabled = false;
        stat1Text.text = Convert.ToInt32(Timer).ToString();
        List<GameObject> troops = tacticalBehavior.GetAllTroops();
        int totalKill = 0;
        foreach (GameObject army in troops)
        {
            totalKill += army.GetComponent<HealthDisplay>().kills;
        }
        stat2Text.text = totalKill.ToString();
        int dieCount = 0;
        foreach (GameObject factroy in GameObject.FindGameObjectsWithTag("UnitFactory"))
        {
            if (factroy.GetComponent<UnitFactory>().hasAuthority)
            {
                UnitFactory localFactory = factroy.GetComponent<UnitFactory>();
                dieCount = localFactory.GetUnitSpawnCount((winner.ToLower() == "blue" ? 0 : 1)) - troops.Count;
            }
        }
        if (winnerObject != null)
        {
            stat3Text.text = Convert.ToInt32(winnerObject.GetComponent<Health>().getCurrentHealth()).ToString();
            stat4Text.text = crownCount.ToString() + " * 500 ";
            stat5Text.text = dieCount.ToString();
            StartCoroutine(updateUserRankingInfo(Convert.ToInt32(Timer), totalKill, Convert.ToInt32(winnerObject.GetComponent<Health>().getCurrentHealth()), dieCount, crownCount));
        }
    }
    private void ClientHandleGameOverdraw()
    {
        winnerNameText.text = $"Draw!";
        gameOverDisplayParent.enabled = true;
    }
    public void ClientHandleGameOverResign()
    {
        Debug.Log("Quit Game");
        if (gameOverDisplayParent == null) { gameOverDisplayParent = GetComponentInChildren<Canvas>(); }
        winnerNameText.text = $"Someone give up";
        gameOverDisplayParent.enabled = true;
        cardDisplay.enabled = false;
    }
    IEnumerator updateUserRankingInfo(int timeleft, int killcount, int health, int dieCount, int crownCount)
    {
        int point = 0;
        point += timeleft;   
        point += killcount;    // Time Left
        point += health;
        point += (crownCount * 500);
        point = point - dieCount;
        totalText.text = point.ToString();

        yield return apiManager.GetEventRanking(StaticClass.EventRankingID, StaticClass.UserID);
        JSONNode jsonResult = apiManager.data["GetEventRanking"];
        int currentEventPoint = 0;
        if (jsonResult.Count > 0) { currentEventPoint = Int32.Parse(jsonResult[0]["point"].ToString().Trim('"'));  }
        Debug.Log($"updateUserRankingInfo point: {point} currentEventPoint : {currentEventPoint}");
        if (point > currentEventPoint)
            yield return apiManager.UpdateEventRanking(StaticClass.UserID, StaticClass.EventRankingID, point.ToString());
        else
            yield return null;
    }
}
