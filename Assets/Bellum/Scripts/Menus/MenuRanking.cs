using System;
using System.Collections;
using System.Text;
using SimpleJSON;
using TMPro;
using UnityEngine;

public class MenuRanking : MonoBehaviour
{
    [SerializeField] public GameObject userRankContentParent = null;
    [SerializeField] public GameObject listMeParent = null;

    APIManager apiManager;
    void Start()
    {
        if (StaticClass.EventRankingID == null || StaticClass.EventRankingID.Length == 0) StaticClass.EventRankingID = "1";
        Debug.Log($"start MenuRanking");
        apiManager = new APIManager();
        StartCoroutine(LoadRankingInfo());
    }
    IEnumerator LoadRankingInfo()
    {
        yield return GetUserRankingInfo(StaticClass.EventRankingID, "");
        yield return GetUserRankingInfo(StaticClass.EventRankingID, StaticClass.UserID);
    }
     // sends an API request - returns a JSON file
    IEnumerator GetUserRankingInfo(string eventid, string userid)
    {
        yield return apiManager.GetEventRanking(eventid, userid);
        JSONNode jsonResult = apiManager.data["GetEventRanking"];
        UserRankItem[] userRank = (userid.Length > 0) ? userRankContentParent.GetComponentsInChildren<UserRankItem>() : listMeParent.GetComponents<UserRankItem>();
        Debug.Log($"userRank item {userRank.Length} {jsonResult}");
        for (int i = 0; i < jsonResult.Count; i++)
        {
            Debug.Log($"jsonresult : userid {jsonResult[i]["userid"].ToString()} point {jsonResult[i]["point"].ToString()}");
            userRank[i].userid.text = jsonResult[i]["userid"].ToString();
            userRank[i].point.text = jsonResult[i]["point"].ToString();
        }

    }
}
