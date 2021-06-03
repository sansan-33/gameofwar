using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mirror;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject landingPagePanel = null;
    private static Dictionary<string, string[]> userTeamDict = new Dictionary<string, string[]>();
    //[SerializeField] public CharacterFullArt characterFullArt;
    [SerializeField] private FirebaseManager firebaseManager;
    [SerializeField] private TopBarMenu topBarMenu = null;
    [SerializeField] public UnitFactory localFactory;
    [SerializeField] public Transform[] unitBodyParents;
    APIManager apiManager;

    private void Awake()
    {
        apiManager = new APIManager();
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
        localFactory.initUnitDict();
        Vector3 unitPos;
        string[] cardkeys = userTeamDict[userTeamDict.Keys.First()];
      
        //if (characterFullArt.CharacterFullArtDictionary.Count < 1){
        //    characterFullArt.initDictionary();
        //}
        for (int i = 0; i < unitBodyParents.Length; i++)
        {
            unitBodyParents[i].gameObject.SetActive(true);
            unitPos = unitBodyParents[i].gameObject.transform.position;
            //teamCardImages[i].sprite = characterFullArt.CharacterFullArtDictionary[cardkeys[i]].image;
            UnitMeta.UnitKey unitKey = (UnitMeta.UnitKey)Enum.Parse(typeof(UnitMeta.UnitKey), cardkeys[i]);
            GameObject unitPrefab = localFactory.GetUnitPrefab(unitKey);
            Transform unitBody = Instantiate(unitPrefab.transform.Find("Body"));
            unitBody.gameObject.GetComponentInChildren<Animator>().enabled = true;
            unitBody.transform.position = new Vector3(unitPos.x , unitPos.y , unitPos.z - 12);
            unitBody.transform.Rotate(-90,90,90);
            unitBody.transform.localScale = new Vector3(7f, 7f, 7f);
            unitBody.transform.SetParent(unitBodyParents[i].transform);
        }
        //Debug.Log($"Load Team Lobby Done. StaticClass.Username: {StaticClass.Username}" );
    }
    // sends an API request - returns a JSON file
    IEnumerator GetTeamInfo(string userid)
    {
        UnitMeta.UnitKey unitkey;
        String comma = "";
        StringBuilder sbTeamMember = new StringBuilder();
        userTeamDict.Clear();
        yield return apiManager.GetTeamInfo(userid);

        JSONNode jsonResult = apiManager.data["GetTeamInfo"];
        for (int i = 0; i < jsonResult.Count; i++)
        {
            if(!userTeamDict.ContainsKey(jsonResult[i]["teamnumber"]))
                userTeamDict.Add(jsonResult[i]["teamnumber"], new string[] { jsonResult[i]["cardkey1"] , jsonResult[i]["cardkey2"], jsonResult[i]["cardkey3"] });

            for (int j = 1; j < 4; j++)
            {
                sbTeamMember.Append(comma).Append(jsonResult[i]["cardkey" + j].ToString());
                unitkey = (UnitMeta.UnitKey)Enum.Parse(typeof(UnitMeta.UnitKey), jsonResult[i]["cardkey" + j] );  
                if (UnitMeta.KeyType[unitkey] == UnitMeta.UnitType.KING)
                {
                    StaticClass.playerRace = UnitMeta.KeyRace[unitkey];
                    //Debug.Log($"StaticClass.playerRace {StaticClass.playerRace}");
                }
                comma = ",";
            }
        }
        sbTeamMember.Replace("\"", "");
        //Debug.Log($"card keys {sbTeamMember.ToString()}");
        yield return GetTotalPower(userid, sbTeamMember.ToString());
    }
    // sends an API request - returns a JSON file
    IEnumerator GetTotalPower(string userid, string cardkeys)
    {
        int TotalPower = 0;
        if (userid == null || userid.Length == 0) { yield break; }

        yield return apiManager.GetTotalPower(userid, cardkeys);
        for (int i = 0; i < apiManager.data["GetTotalPower"].Count; i++)
        {
            TotalPower += Int32.Parse(apiManager.data["GetTotalPower"][i]["power"]);
        }
        StaticClass.TotalPower = TotalPower.ToString();
        //Debug.Log($"TotalPower {TotalPower}");

    }
}
