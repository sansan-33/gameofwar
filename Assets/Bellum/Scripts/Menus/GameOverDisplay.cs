using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Mirror;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverDisplay : MonoBehaviour
{
    [SerializeField] private GameObject winCanvas;
    [SerializeField] private GameObject loseCanvas;
    [SerializeField] private GameObject winResult;
    [SerializeField] private GameObject OpenCanvasButton;

    [SerializeField] public Canvas gameOverDisplayParent;
    [SerializeField] private TMP_Text winnerNameText = null;
    [SerializeField] private TMP_Text stat1Text = null;
    [SerializeField] private TMP_Text stat2Text = null;
    [SerializeField] private TMP_Text stat3Text = null;
    [SerializeField] private TMP_Text stat4Text = null;
    [SerializeField] private TMP_Text stat5Text = null;
    [SerializeField] private TMP_Text stat6Text = null;

    [SerializeField] private TMP_Text totalText = null;

    [SerializeField] private Canvas cardDisplay = null;
    [SerializeField] private Canvas spButtonDisplay = null;
    [SerializeField] private TMP_Text crownBlueText = null;
    [SerializeField] private TMP_Text crownRedText = null;
    [SerializeField] public GameStartDisplay gameStartDisplay;
    [SerializeField] public GameObject menuDisplay;

    [Header("Rewards Item")]
    [SerializeField] private TMP_Text rewardExperience = null;
    [SerializeField] private TMP_Text rewardGold = null;
    [SerializeField] private GameObject rewardGem = null;
    [SerializeField] private TMP_Text rewardGemCount = null;

    public TacticalBehavior tacticalBehavior;
    private RTSPlayer rTSPlayer;
    APIManager apiManager;
    private bool IS_COMPLETED = false;
    private bool openedCanvas = false;
    private int winnerID;

    private void FixedUpdate()
    {
        if(gameStartDisplay.GetGameTimerValue() <= 0  && !IS_COMPLETED) {
            int blueCrown = Int32.Parse(crownBlueText.text);
            int redCrown = Int32.Parse(crownRedText.text);
            //Debug.Log($"GameOver Display timer: {gameStartDisplay.GetGameTimerValue()} , IS_COMPLETED ? {IS_COMPLETED} blueCrown:{blueCrown} redCrown:{redCrown}");

            if (blueCrown != redCrown && (blueCrown + redCrown) > 0 ) {
                ClientHandleGameOver(blueCrown > redCrown ? "blue" : "red");
            } else
                ClientHandleGameOverdraw();
        }
    }
    private void Start()
    {
        rTSPlayer = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
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
        double timer = gameStartDisplay.GetGameTimerValue();
        //Debug.Log($" ClientHandleGameOver winner: {winner} , Timer: {Timer}");
        int _playerid = (winner.ToLower() == "blue" ? 0 : 1);
        winnerID = _playerid;
        GameObject winnerObject = GameObject.FindGameObjectWithTag("King" + _playerid);
        winnerNameText.text = $"{winner} Team";
        gameOverDisplayParent.enabled = true;

        CinemachineManager cmManager = GameObject.FindGameObjectWithTag("CinemachineManager").GetComponent<CinemachineManager>();
        cmManager.ThirdCamera(winnerObject, winnerObject);
        winnerNameText.DOFade(0f, 0.1f);
        winnerNameText.DOFade(1f, 5f);
        winnerNameText.transform.DOMove(winnerNameText.transform.position + 2 * (Vector3.down), 1.75f).OnComplete(() => {
            //Destroy(transform.root.gameObject);
        });
        winnerNameText.color = winner.ToLower() == "blue" ? Color.blue : Color.red;
        int crownCount = winner.ToLower() == "blue" ? Int32.Parse(crownBlueText.text) : Int32.Parse(crownRedText.text);

        cardDisplay.enabled = false;
        menuDisplay.SetActive(false);
        spButtonDisplay.enabled = false;
        stat1Text.text = Convert.ToInt32(timer).ToString();
        List<Unit> troops = tacticalBehavior.GetAllTroops(_playerid);
        int totalKill = 0;
        foreach (Unit army in troops)
        {
            if(army.GetComponent<HealthDisplay>() != null)
            totalKill += army.GetComponent<HealthDisplay>().kills;
            //Debug.Log($"totalKill = {totalKill} + {army.name} kill {army.GetComponent<HealthDisplay>().kills}");
        }
        stat2Text.text = totalKill.ToString();
        int dieCount = 0;
        foreach (GameObject factroy in GameObject.FindGameObjectsWithTag("UnitFactory"))
        {
            if (factroy.GetComponent<UnitFactory>().hasAuthority)
            {
                UnitFactory localFactory = factroy.GetComponent<UnitFactory>();
                dieCount = localFactory.GetUnitSpawnCount(_playerid) - troops.Count;
            }
        }
        if (winnerObject != null)
        {
            stat3Text.text = Convert.ToInt32(winnerObject.GetComponent<Health>().getCurrentHealth()).ToString();
            stat4Text.text = crownCount.ToString() + " * 500 ";
            stat5Text.text = dieCount.ToString();
            stat6Text.text = StaticClass.HighestDamage.ToString() + " * 30 ";
            StartCoroutine(updateUserRankingInfo(Convert.ToInt32(timer), totalKill, Convert.ToInt32(winnerObject.GetComponent<Health>().getCurrentHealth()), dieCount, crownCount));
            StartCoroutine(updateUserReward());
            IS_COMPLETED = true;
        }
        OpenCanvasButton.SetActive(true);
        Debug.Log($"_playerid = {_playerid} enemy id = {rTSPlayer.GetEnemyID()}player id = {rTSPlayer.GetPlayerID()}");
        if (_playerid == rTSPlayer.GetPlayerID())
        {
            winCanvas.SetActive(true);
        }
        else
        {
            loseCanvas.SetActive(true);
        }
    }
    public void HandleResultCanvas()
    {
        Debug.Log($"HandleResultCanvas winner is {winnerID}");
        if (winnerID == rTSPlayer.GetPlayerID())
        {
            if(openedCanvas == true)
            {
                openedCanvas = false;
                winResult.SetActive(false);
            }
            else
            {
                openedCanvas = true;
                winResult.SetActive(true);
            }
           
        }
    }
    private void ClientHandleGameOverdraw()
    {
        //Debug.Log($"Game Draw ");
        winnerNameText.text = $"Draw!";
        gameOverDisplayParent.enabled = true;
        cardDisplay.enabled = false;
        menuDisplay.SetActive(false);
        IS_COMPLETED = true;
    }
    public void ClientHandleGameOverResign()
    {
        //Debug.Log("Quit Game");
        if (gameOverDisplayParent == null) { gameOverDisplayParent = GetComponentInChildren<Canvas>(); }
        winnerNameText.text = $"Someone give up";
        gameOverDisplayParent.enabled = true;
        cardDisplay.enabled = false;
        menuDisplay.SetActive(false);
        IS_COMPLETED = true;
    }
    IEnumerator updateUserRankingInfo(int timeleft, int killcount, int health, int dieCount, int crownCount)
    {
        int point = 0;
        point += (timeleft * 3);   
        point += killcount;    
        point += health;
        point += (crownCount * 500);
        point += Convert.ToInt32(StaticClass.HighestDamage) * 30;
        point = point - dieCount;
        point = point * 1 + ( (Int32.Parse(StaticClass.Mission) - 1) / 10);
        totalText.text = point.ToString();

        yield return apiManager.GetEventRanking(StaticClass.EventRankingID, StaticClass.UserID);
        JSONNode jsonResult = apiManager.data["GetEventRanking"];
        int currentEventPoint = 0;
        if (jsonResult.Count > 0) { currentEventPoint = Int32.Parse(jsonResult[0]["point"].ToString().Trim('"'));  }
        //Debug.Log($"updateUserRankingInfo point: {point} currentEventPoint : {currentEventPoint}");
        if (point > currentEventPoint)
            yield return apiManager.UpdateEventRanking(StaticClass.UserID, StaticClass.EventRankingID, point.ToString());
        else
            yield return null;
    }
    IEnumerator updateUserReward()
    {
        string missionKey = StaticClass.Chapter + "-" + StaticClass.Mission;
        int randCount = UnityEngine.Random.Range(1, 2);
        rewardExperience.text = RewardMeta.missionExp[missionKey].ToString();
        rewardGold.text = RewardMeta.missionGold[missionKey].ToString();
        string gemType = RewardMeta.missionTreasure[missionKey].ToString();
        var gem = gemType.Split('-')[0];
        var gemRate = gemType.Split('-')[1];
    
        rewardGem.transform.Find("icon_" + gem).gameObject.SetActive(true);
        rewardGemCount.text = randCount.ToString();

        yield return apiManager.UpdateUserReward(StaticClass.UserID, RewardMeta.missionExp[missionKey].ToString() , RewardMeta.missionGold[missionKey].ToString(), gem, randCount.ToString());
    }
}
