using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mirror;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject landingPagePanel = null;
    [SerializeField] public TMP_Text userid = null;
    [SerializeField] public Image[] teamCardImages = new Image[3];
    private Dictionary<string, string[]> userTeamDict = new Dictionary<string, string[]>();
    [SerializeField] public CharacterFullArt characterFullArt;
    [SerializeField] private LoginManager loginManager;

    private void Awake()
    {
        loginManager.loginChanged += HandleLoadTeam;
    }
    private void OnDestroy()
    {
        loginManager.loginChanged -= HandleLoadTeam;
    }

    public void Start()
    {
        if (StaticClass.UserID == null || StaticClass.UserID.Length == 0)
            StaticClass.UserID = "1";

        userid.text = StaticClass.UserID;
        HandleLoadTeam();
    }
    public void HostLobby()
    {
        landingPagePanel.SetActive(false);

        NetworkManager.singleton.StartHost();
    }
    public void HandleLoadTeam()
    {
        StartCoroutine(LoadTeamLobby());
    }
    IEnumerator LoadTeamLobby(){
        yield return GetTeamInfo(StaticClass.UserID);
        string[] cardkeys = userTeamDict[userTeamDict.Keys.First()];
        if (characterFullArt.CharacterFullArtDictionary.Count < 1){
            characterFullArt.initDictionary();
        }
        for (int i = 0; i < teamCardImages.Length; i++) {
            teamCardImages[i].sprite = characterFullArt.CharacterFullArtDictionary[cardkeys[i]].image;
        }
    }
    // sends an API request - returns a JSON file
    IEnumerator GetTeamInfo(string userid)
    {
        userTeamDict.Clear();
        // resulting JSON from an API request
        JSONNode jsonResult;
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();

        // build the url and query

        webReq.url = string.Format("{0}/{1}/{2}", APIConfig.urladdress, APIConfig.teamService, userid);

        // send the web request and wait for a returning result
        yield return webReq.SendWebRequest();

        // convert the byte array to a string
        string rawJson = Encoding.Default.GetString(webReq.downloadHandler.data);

        // parse the raw string into a json result we can easily read
        jsonResult = JSON.Parse(rawJson);
        for (int i = 0; i < jsonResult.Count; i++)
        {
            userTeamDict.Add(jsonResult[i]["teamnumber"], new string[] { jsonResult[i]["cardkey1"] , jsonResult[i]["cardkey2"], jsonResult[i]["cardkey3"] });
        }
        Debug.Log($"jsonResult {webReq.url } {jsonResult}");

    }
}
