using System;
using System.Collections;
using System.Text;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Localization.Settings;


public class TeamManager : MonoBehaviour
{
    [SerializeField] public CharacterArt Arts;
    [SerializeField] public UnitTypeArt unitTypeArt;
    public UserCardManager userCardManager;
    private Dictionary<string, UserCard[]> userTeamDict = new Dictionary<string, UserCard[]>();
    public GameObject TeamCardSlot;
    public PopupMessageDisplay popupMessageDisplay;

    private static int NOOFCARDSLOT = 3;
    private string teamnumber= "0";
    private UserCard[] userCardArray = new UserCard[NOOFCARDSLOT];

    public void Start()
    {
        UserCardManager.UserCardLoaded += LoadTeam;
        TeamTabButton.TeamTabChanged += TeamChanged;
    }
    public void OnDestroy()
    {
        UserCardManager.UserCardLoaded -= LoadTeam;
        TeamTabButton.TeamTabChanged -= TeamChanged;
    }
    public void LoadTeam(string userid)
    {
        StartCoroutine(HandleLoadTeam(userid));
    }
    IEnumerator HandleLoadTeam(string userid)
    {
        yield return GetTeamInfo(userid);
        TeamCardButton teamCardBtn;
        UnitMeta.UnitKey unitkey;
        //UnitMeta.UnitKey[] teamMembers = new UnitMeta.UnitKey [NOOFCARDSLOT];
        UserCard[] userCardArray = new UserCard[NOOFCARDSLOT];
        for (int i = 0; i < NOOFCARDSLOT; i++)
            TeamCardSlot.transform.GetChild(i).GetComponent<TeamCardButton>().cardSlotEmpty.SetActive(true);
        //Debug.Log($"Handle Load Team -- teamnumber {teamnumber}");
        if (userTeamDict.TryGetValue( teamnumber , out userCardArray))
        {
            //Debug.Log($"userTeamDict -- teamnumber {teamnumber} {userCardArray} ");
            for (int i = 0; i < NOOFCARDSLOT; i++)
            {
                teamCardBtn = TeamCardSlot.transform.GetChild(i).GetComponent<TeamCardButton>();
                //Debug.Log($"TeamManager.HandleLoadTeam() i:{i} userCardArray[i].cardkey: {userCardArray[i].cardkey}");
                teamCardBtn.characterImage.sprite = Arts.CharacterArtDictionary[userCardArray[i].cardkey].image;

                // Localization
                //teamCardBtn.cardSlotKey.text = userCardArray[i].cardkey;
                AsyncOperationHandle<string> op = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(LanguageSelectionManager.STRING_TEXT_REF, userCardArray[i].cardkey.ToLower(), null);
                if (op.IsDone)
                {
                    teamCardBtn.cardSlotKey.text = op.Result;
                }
                else
                {
                    op.Completed += (o) => teamCardBtn.cardSlotKey.text = o.Result;
                }
                teamCardBtn.cardSlotKey.font = userCardManager.localizationResponder.getCurrentFont();

                //teamCardBtn.cardSlotType.text = userCardArray[i].unittype;
                op = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(LanguageSelectionManager.STRING_TEXT_REF, userCardArray[i].unittype.ToLower(), null);
                if (op.IsDone)
                {
                    teamCardBtn.cardSlotType.text = op.Result;
                }
                else
                {
                    op.Completed += (o) => teamCardBtn.cardSlotType.text = o.Result;
                }
                teamCardBtn.cardSlotType.font = userCardManager.localizationResponder.getCurrentFont();
                //Debug.Log($"TeamManager.HandleLoadTeam() teamCardBtn.cardSlotType.text:{teamCardBtn.cardSlotType.text}");

                teamCardBtn.cardSlotLevel.text = userCardArray[i].level;
                teamCardBtn.unitTypeImage.sprite = unitTypeArt.UnitTypeArtDictionary[userCardArray[i].unittype].image;
                
                teamCardBtn.cardSlotEmpty.SetActive(false);
                //teamMembers[i] = (UnitMeta.UnitKey) Enum.Parse(typeof(UnitMeta.UnitKey), userCardArray[i].cardkey);
                unitkey = (UnitMeta.UnitKey)Enum.Parse(typeof(UnitMeta.UnitKey), userCardArray[i].cardkey);
                if (UnitMeta.KeyType[unitkey] == UnitMeta.UnitType.KING )
                {
                    StaticClass.playerRace = UnitMeta.KeyRace[unitkey];
                    Debug.Log($"StaticClass.playerRace {StaticClass.playerRace}");
                }
            }
            //StaticClass.teamMembers = teamMembers;
            
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
            userCardArray = new UserCard[NOOFCARDSLOT];
            for (int j=0;j<NOOFCARDSLOT;j++) {
                if (jsonResult[i]["cardkey" + (j + 1)] != null)
                    userCardArray[j] = userCardManager.userCardDict[jsonResult[i]["cardkey" + (j+1) ]];
            }
            if (jsonResult[i]["teamnumber"] != null)
                userTeamDict.Add(jsonResult[i]["teamnumber"], userCardArray);
        }
        //Debug.Log($"jsonResult {webReq.url } {jsonResult}");

    }
    public void SaveTeamFormation()
    {
        bool HasOneKing = false;
        string[] cardSlotKeys = new string[NOOFCARDSLOT];
        TeamCardButton teamCardBtn;
        for (int i = 0; i < NOOFCARDSLOT; i++)
        {
            teamCardBtn = TeamCardSlot.transform.GetChild(i).GetComponent<TeamCardButton>();
            cardSlotKeys[i] = teamCardBtn.cardSlotKey.text;
            if(UnitMeta.KeyType[(UnitMeta.UnitKey)Enum.Parse(typeof(UnitMeta.UnitKey), cardSlotKeys[i])] == UnitMeta.UnitType.KING){
                HasOneKing = true;
            }
        }
        if (HasOneKing) {
            StartCoroutine(handleSaveTeamMember(StaticClass.UserID, cardSlotKeys[0], cardSlotKeys[1], cardSlotKeys[2]));
        } else {
            popupMessageDisplay.displayText(2f, "Please select a King", false);
        }
    }
    public void TeamChanged(string local_teamnumber)
    {
        //Debug.Log($"Tab Menu Clicked {local_teamnumber}");
        teamnumber = local_teamnumber;
        StartCoroutine(HandleLoadTeam(StaticClass.UserID));
    }
    IEnumerator handleSaveTeamMember(string userid, string cardkey1, string cardkey2, string cardkey3)
    {
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();
        JSONNode jsonResult;
        //   "/team/{userid}/{cardkey1}/{cardkey2}/{cardkey3}/{teamnumber}" , method = RequestMethod.PUT )
        webReq.url = string.Format("{0}/{1}/{2}/{3}/{4}/{5}/{6}", APIConfig.urladdress, APIConfig.teamService, userid, teamnumber, cardkey1, cardkey2, cardkey3);
        webReq.method = "put";
        Debug.Log($"levelup card {webReq.url }");
        // send the web request and wait for a returning result
        yield return webReq.SendWebRequest();
        string rawJson = Encoding.Default.GetString(webReq.downloadHandler.data);
        Debug.Log($"rawJson {rawJson}");
        jsonResult = JSON.Parse(rawJson); 
        string status = jsonResult["status"];
        popupMessageDisplay.displayText(2f,"Team Saved [" + status + "]"  , status.Contains("OK"));
    }
}

