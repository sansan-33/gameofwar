using System;
using System.Collections;
using System.Text;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class TeamManager : MonoBehaviour
{
    [SerializeField] public CharacterArt Arts;
    [SerializeField] public UnitTypeArt unitTypeArt;
    public UserCardManager userCardManager;
    private Dictionary<string, UserCard[]> userTeamDict = new Dictionary<string, UserCard[]>();
    public GameObject TeamCardSlot;
    
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
        UnitMeta.UnitKey[] teamMembers = new UnitMeta.UnitKey [NOOFCARDSLOT];
        UserCard[] userCardArray = new UserCard[NOOFCARDSLOT];
        for (int i = 0; i < NOOFCARDSLOT; i++)
            TeamCardSlot.transform.GetChild(i).GetComponent<TeamCardButton>().cardSlotEmpty.SetActive(true);
        Debug.Log($"Handle Load Team -- teamnumber {teamnumber}");
        if (userTeamDict.TryGetValue( teamnumber , out userCardArray))
        {
            Debug.Log($"userTeamDict -- teamnumber {teamnumber} {userCardArray} ");
            for (int i = 0; i < NOOFCARDSLOT; i++)
            {
                teamCardBtn = TeamCardSlot.transform.GetChild(i).GetComponent<TeamCardButton>();
                teamCardBtn.characterImage.sprite = Arts.CharacterArtDictionary[userCardArray[i].cardkey].image;
                teamCardBtn.cardSlotKey.text = userCardArray[i].cardkey;
                teamCardBtn.cardSlotLevel.text = userCardArray[i].level;
                teamCardBtn.unitTypeImage.sprite = unitTypeArt.UnitTypeArtDictionary[userCardArray[i].unittype].image;
                teamCardBtn.cardSlotType.text = userCardArray[i].unittype;
                teamCardBtn.cardSlotEmpty.SetActive(false);
                teamMembers[i] = (UnitMeta.UnitKey) Enum.Parse(typeof(UnitMeta.UnitKey), userCardArray[i].cardkey);
            }
            StaticClass.teamMembers = teamMembers;
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
                userCardArray[j] = userCardManager.userCardDict[jsonResult[i]["cardkey" + (j+1) ]];
            }
            userTeamDict.Add(jsonResult[i]["teamnumber"], userCardArray);
        }
        Debug.Log($"jsonResult {webReq.url } {jsonResult}");

    }
    public void SaveTeamFormation()
    {
        string[] cardSlotKeys = new string[NOOFCARDSLOT];
        TeamCardButton teamCardBtn;
        for (int i = 0; i < NOOFCARDSLOT; i++)
        {
            teamCardBtn = TeamCardSlot.transform.GetChild(i).GetComponent<TeamCardButton>();
            cardSlotKeys[i] = teamCardBtn.cardSlotKey.text;
        }
        StartCoroutine(handleSaveTeamMember(StaticClass.UserID, cardSlotKeys[0], cardSlotKeys[1], cardSlotKeys[2]));
    }
    public void TeamChanged(string local_teamnumber)
    {
        Debug.Log($"Tab Menu Clicked {local_teamnumber}");
        teamnumber = local_teamnumber;
        StartCoroutine(HandleLoadTeam(StaticClass.UserID));
    }
    IEnumerator handleSaveTeamMember(string userid, string cardkey1, string cardkey2, string cardkey3)
    {
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();

        //   "/team/{userid}/{cardkey1}/{cardkey2}/{cardkey3}/{teamnumber}" , method = RequestMethod.PUT )
        webReq.url = string.Format("{0}/{1}/{2}/{3}/{4}/{5}/{6}", APIConfig.urladdress, APIConfig.teamService, userid, teamnumber, cardkey1, cardkey2, cardkey3);
        webReq.method = "put";
        Debug.Log($"levelup card {webReq.url }");
        // send the web request and wait for a returning result
        yield return webReq.SendWebRequest();
    }
}

