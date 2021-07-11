using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameStartDisplay : NetworkBehaviour
{
    [SerializeField] private GameObject gameStartDisplayParent = null;
    [SerializeField] private TMP_Text StartTime = null;
    [SerializeField] private TMP_Text Times = null;

    [SerializeField] private GameObject playerVSParent = null;
    [SerializeField] private GameObject maskBlue = null;
    [SerializeField] private GameObject maskRed = null;
    [SerializeField] private GameObject vsFrame = null;
    [SerializeField] private GameObject vsText = null;
    [SerializeField] public CharacterArt Arts;
    public static event Action ServerGameStart;
    public static event Action ServerGameSpeedUp;

    private double Timer = 180;
    private double startTimer = 3;
    bool IS_PLAYER_LOADED = false;
    RTSPlayer player;
    double SPEEPUPTIME = 60; // will speed up eleixier recovery after 10s
    bool ISSPEEDUP = false;
    double offset=0;
    double now=0;

    public override void OnStartClient()
    {
        if (NetworkClient.connection.identity == null) { return; }
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        StartCoroutine(StartGameloading());
        
    }
    private void FixedUpdate()
    {
        if (offset > 0){
            now = Timer - (NetworkTime.time - offset);
        }
        GameStartCountDown();
        GameEndCountDown();
    }
    IEnumerator StartGameloading()
    {
        yield return LoadPlayerVS();
    }
    IEnumerator LoadPlayerVS()
    {
        if (playerVSParent == null || IS_PLAYER_LOADED) { yield break; }
        playerVSParent.SetActive(true);
        StartCoroutine(LoadPlayerData());
        StartCoroutine(LerpPosition(maskBlue.transform , 400f,0f, .5f));
        yield return LerpPosition(maskRed.transform, -400f, 0f, .5f);
        //StartCoroutine(LerpPosition(vsFrame.transform, 2000f, 2000f, .5f));
        vsText.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        startTimer += NetworkTime.time;
        IS_PLAYER_LOADED = true;
        playerVSParent.SetActive(false);

    }
    IEnumerator LerpPosition(Transform transformObject, float targetPointX, float targetPointY, float duration)
    {
        float time = 0;
        Vector3 targetPosition = transformObject.position;
        //Vector3 targetPosition = centerPoint.position;
        Vector3 startPosition = new Vector3 ( transformObject.position.x + targetPointX , transformObject.position.y + targetPointY, transformObject.position.z) ;
        transformObject.position = startPosition;
        transformObject.gameObject.SetActive(true);
        //Vector3 dir = targetPosition - startPosition;
        //transformObject.LookAt(dir);
        while (time < duration)
        {
            transformObject.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            if(targetPointY > 0)
            LerpRotation(Quaternion.LookRotation(targetPosition - startPosition), duration);
            time += Time.deltaTime;
            yield return null;
        }
        transformObject.position = targetPosition;
    }
    IEnumerator LerpRotation(Quaternion endValue, float duration)
    {
        float time = 0;
        Quaternion startValue = transform.rotation;

        while (time < duration)
        {
            transform.rotation = Quaternion.Lerp(startValue, endValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.rotation = endValue;
    }
    IEnumerator LoadPlayerData()
    {
        GameObject[] teamInfo = { maskBlue, maskRed };
        UnitMeta.Race playerRace;
        List<RTSPlayer> players = ((RTSNetworkManager)NetworkManager.singleton).Players;
        for (int i = 0; i < players.Count; i++)
        {
            playerRace = (UnitMeta.Race)Enum.Parse(typeof(UnitMeta.Race),  players[i].GetRace());
            //Debug.Log($"LoadPlayerData userid {players[i].GetUserID()} race {players[i].GetRace() } total power {players[i].GetTotalPower() }");
            teamInfo[i].GetComponent<PlayerVS>().PlayerName.text = players[i].GetDisplayName() ;
            teamInfo[i].GetComponent<SlidingNumber>().SetNumber(Int32.Parse(players[i].GetTotalPower()));
            teamInfo[i].GetComponent<PlayerVS>().charIcon.GetComponent<Image>().sprite = Arts.CharacterArtDictionary[UnitMeta.UnitRaceTypeKey[playerRace][UnitMeta.UnitType.KING].ToString()].image;
            teamInfo[i].GetComponent<PlayerVS>().charIcon.GetComponent<Image>().enabled = true;
            teamInfo[i].SetActive(true);
        }
        if (players.Count == 1)
        {
            if (StaticClass.Chapter == null)
            {
                StaticClass.Chapter = "1";
                StaticClass.Mission = "1";
            }
            UnitMeta.Race race = StaticClass.Chapter == null ? UnitMeta.Race.ELF : (UnitMeta.Race)Enum.Parse(typeof(UnitMeta.Race), (int.Parse(StaticClass.Chapter) - 1).ToString());
            int TotalPower = 0;
            APIManager apiManager = new APIManager();
            //Debug.Log($"Chapter Mission Team {StaticClass.Chapter + " - " + StaticClass.Mission}");
            yield return apiManager.GetTotalPower(UnitMeta.ENEMY_USERID, ChapterMissionMeta.ChapterMissionTeam[StaticClass.Chapter + "-" + StaticClass.Mission]);
            for (int i = 0; i < apiManager.data.Count; i++)
            {
                TotalPower += Int32.Parse(apiManager.data["GetTotalPower"][i]["power"]);
            }
            maskRed.GetComponent<PlayerVS>().charIcon.GetComponent<Image>().sprite = Arts.CharacterArtDictionary[UnitMeta.UnitRaceTypeKey[race][UnitMeta.UnitType.KING].ToString()].image;
            maskRed.GetComponent<PlayerVS>().charIcon.GetComponent<Image>().enabled = true;
            maskRed.GetComponent<PlayerVS>().PlayerName.text = race.ToString() + StaticClass.Mission;
            maskRed.GetComponent<SlidingNumber>().SetNumber(TotalPower);
            //maskRed.GetComponent<PlayerVS>().TotalPower.text = TotalPower.ToString();
            maskRed.SetActive(true);
        }
    }
    private void GameStartCountDown()
    {
        if (IS_PLAYER_LOADED && offset==0)
            StartTiming();
    }
    private void GameEndCountDown()
    {
        if (IS_PLAYER_LOADED)
            Timing();
    }
    public void StartTiming()
    {
        //Debug.Log($"StartTiming startTimer {startTimer} NetworkTime.time {NetworkTime.time}");
        if (startTimer > NetworkTime.time - 1 && startTimer < NetworkTime.time)
        {
            gameStartDisplayParent.SetActive(true);
            StartTime.text = "Fight!";
        }
        else if (startTimer >= NetworkTime.time)
        {
            offset = NetworkTime.time;
            gameStartDisplayParent.SetActive(false);
            ServerGameStart?.Invoke();
        }
    }
    public void Timing()
    {
        if (offset <= 0.1) { return; }
        if (now <= 0) { return; }
        if (now <= Timer - SPEEPUPTIME && !ISSPEEDUP) { Debug.Log($"now{now} Timer{Timer} - SPEEPUPTIME{SPEEPUPTIME}={Timer - SPEEPUPTIME}, ISSPEEDUP ? {ISSPEEDUP} "); ServerGameSpeedUp?.Invoke(); ISSPEEDUP = true; }
        int minutes = Convert.ToInt32(now) / 60;
        float seconds = Convert.ToInt32(now % 60);
        seconds = (seconds == 60 || seconds<=0) ? 0 : seconds;
        minutes = (minutes <= 0) ? 0 : minutes;
        //Debug.Log($"Timing now {now} , minutes:{minutes}, seconds:{seconds}" );
        Times.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    public string GetGameTimer()
    {
        return now.ToString();
    }
    public double GetGameTimerValue()
    {
        return now == 0 ? Timer : now ; // not return 0 at start up , otherwise game draw immediately
    } 
}
