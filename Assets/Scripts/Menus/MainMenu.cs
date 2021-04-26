using System;
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
    [SerializeField] public Image[] teamCardImages = new Image[3];
    private static Dictionary<string, string[]> userTeamDict = new Dictionary<string, string[]>();
    [SerializeField] public CharacterFullArt characterFullArt;
    [SerializeField] private FirebaseManager firebaseManager;
    [SerializeField] private TopBarMenu topBarMenu = null;

    private void Awake()
    {
        firebaseManager.authStateChanged += HandleLobbyData;
    }
    private void OnDestroy()
    {
        firebaseManager.authStateChanged -= HandleLobbyData;
    }

    public void HostLobby()
    {
        landingPagePanel.SetActive(false);

        NetworkManager.singleton.StartHost();
    }
    public void HandleLobbyData()
    {
       StartCoroutine(LoadLobbyInfo());
    }
    IEnumerator LoadLobbyInfo()
    {
         yield return GetTeamInfo(StaticClass.UserID);
       
        if (userTeamDict.Count < 1) { yield break; }
        string[] cardkeys = userTeamDict[userTeamDict.Keys.First()];
        if (characterFullArt.CharacterFullArtDictionary.Count < 1){
            characterFullArt.initDictionary();
        }
        for (int i = 0; i < teamCardImages.Length; i++) {
            teamCardImages[i].gameObject.SetActive(true);
            teamCardImages[i].sprite = characterFullArt.CharacterFullArtDictionary[cardkeys[i]].image;
        }
        //Debug.Log($"Load Team Lobby Done. StaticClass.Username: {StaticClass.Username}" );
    }
    // sends an API request - returns a JSON file
    IEnumerator GetTeamInfo(string userid)
    {
        UnitMeta.UnitKey unitkey;
        if (userid == null || userid.Length == 0) { yield break; }
        userTeamDict.Clear();
        //Debug.Log($"Get Team info after clear userTeamDict size: {userTeamDict.Count }");
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
            if(!userTeamDict.ContainsKey(jsonResult[i]["teamnumber"]))
                userTeamDict.Add(jsonResult[i]["teamnumber"], new string[] { jsonResult[i]["cardkey1"] , jsonResult[i]["cardkey2"], jsonResult[i]["cardkey3"] });

            for (int j = 1; j < 4; j++)
            {
                unitkey = (UnitMeta.UnitKey)Enum.Parse(typeof(UnitMeta.UnitKey), jsonResult[i]["cardkey" + j] );  
                if (UnitMeta.KeyType[unitkey] == UnitMeta.UnitType.KING)
                {
                    StaticClass.playerRace = UnitMeta.KeyRace[unitkey];
                    //Debug.Log($"StaticClass.playerRace {StaticClass.playerRace}");
                }
            }
        }
        //Debug.Log($"Team info {webReq.url } {jsonResult}");

    }
}
