using System;
using System.Collections;
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


    [SyncVar(hook = "StartTiming")]
    private float startTime = 1;
    [SyncVar(hook = "Timing")]
    private float Timer = 180;
    bool IS_PLAYER_LOADED = false;
    RTSPlayer player;

    public override void OnStartClient()
    {
        if (NetworkClient.connection.identity == null) { return; }
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        StartCoroutine(StartGameloading());
    }
    private void Update()
    {
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
        maskBlue.GetComponent<PlayerVS>().PlayerName.text = StaticClass.Username;
        maskBlue.GetComponent<SlidingNumber>().SetNumber(Int32.Parse(StaticClass.TotalPower));
        //maskBlue.GetComponent<PlayerVS>().TotalPower.text = StaticClass.TotalPower;
        maskBlue.GetComponent<PlayerVS>().charIcon.GetComponent<Image>().sprite = Arts.CharacterArtDictionary[UnitMeta.UnitRaceTypeKey[StaticClass.playerRace][UnitMeta.UnitType.KING].ToString()].image;
        maskBlue.GetComponent<PlayerVS>().charIcon.GetComponent<Image>().enabled = true;
        maskBlue.SetActive(true);
        if (StaticClass.Chapter == null) {
            StaticClass.Chapter = "1";
            StaticClass.Mission = "1";
        }
        UnitMeta.Race race = StaticClass.Chapter == null ? UnitMeta.Race.ELF : (UnitMeta.Race)Enum.Parse(typeof(UnitMeta.Race), (int.Parse(StaticClass.Chapter) - 1).ToString());
        int TotalPower=0;
        APIManager apiManager = new APIManager();
        Debug.Log($"Chapter Mission Team {StaticClass.Chapter + " - " + StaticClass.Mission}");
        yield return apiManager.GetTotalPower(UnitMeta.ENEMY_USERID , ChapterMissionMeta.ChapterMissionTeam[StaticClass.Chapter + "-" + StaticClass.Mission]);
        for (int i = 0; i < apiManager.data.Count; i++) {
            TotalPower += Int32.Parse(apiManager.data["GetTotalPower"][i]["power"]);
        }
        maskRed.GetComponent<PlayerVS>().charIcon.GetComponent<Image>().sprite = Arts.CharacterArtDictionary[UnitMeta.UnitRaceTypeKey[race][UnitMeta.UnitType.KING].ToString()].image;
        maskRed.GetComponent<PlayerVS>().charIcon.GetComponent<Image>().enabled = true;
        maskRed.GetComponent<PlayerVS>().PlayerName.text = race.ToString() + StaticClass.Mission;
        maskRed.GetComponent<SlidingNumber>().SetNumber(TotalPower);
        //maskRed.GetComponent<PlayerVS>().TotalPower.text = TotalPower.ToString();
        maskRed.SetActive(true);


    }
    private void GameStartCountDown()
    {
        if(startTime > 0 && IS_PLAYER_LOADED)
            startTime -= 1 * Time.deltaTime;
    }
    private void GameEndCountDown()
    {
        if(IS_PLAYER_LOADED)
        Timer -= Time.deltaTime;
    }
    public void StartTiming(float oldTime, float newTime)
    {
       // Debug.Log($"oldTime:{oldTime}newTime:{newTime}");
        if (newTime <= 30 && newTime > 0)
        {
            gameStartDisplayParent.SetActive(true);
            //StartTime.text = newTime.ToString("0");
            StartTime.text = "Fight!";
        }
        else if (newTime <= 0)
        {
            gameStartDisplayParent.SetActive(false);
            ServerGameStart?.Invoke();
        }
    }
    public void Timing(float oldTime, float newTime)
    {
        //Debug.Log($"oldTime:{oldTime}newTime:{newTime}");
        float minutes = Mathf.FloorToInt(newTime / 60);
        float seconds = Mathf.FloorToInt(newTime % 60);
        if (newTime <= 0) { return; }
        Times.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
     
}
