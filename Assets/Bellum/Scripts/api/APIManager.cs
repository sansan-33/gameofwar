using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;

public class APIManager 
{
    public Dictionary<string,JSONNode> data = new Dictionary<string, JSONNode>();
    // sends an API request - returns a JSON file
    public IEnumerator GetTotalPower(string userid, string cardkeys)
    {
        if (userid == null || userid.Length == 0) { yield break; }
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();
        webReq.url = string.Format("{0}/{1}/{2}/{3}", APIConfig.urladdress, APIConfig.cardService, userid, cardkeys);
        yield return webReq.SendWebRequest();
        string rawJson = Encoding.Default.GetString(webReq.downloadHandler.data);
        if (data.ContainsKey("GetTotalPower"))
        {
            data["GetTotalPower"] = JSON.Parse(rawJson);
        }
        else
        {
            data.Add("GetTotalPower", JSON.Parse(rawJson));

        }
    }
    public IEnumerator GetTeamInfo(string userid)
    {
        if (userid == null || userid.Length == 0) { yield break; }

        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();
        webReq.url = string.Format("{0}/{1}/{2}", APIConfig.urladdress, APIConfig.teamService, userid);
        yield return webReq.SendWebRequest();
        string rawJson = Encoding.Default.GetString(webReq.downloadHandler.data);
        if (data.ContainsKey("GetTeamInfo"))
        {
            data["GetTeamInfo"] = JSON.Parse(rawJson);
        }
        else
        {
            data.Add("GetTeamInfo", JSON.Parse(rawJson));
        }
    }
    public IEnumerator GetEventRanking(string eventid, string userid)
    {
        //Debug.Log($"GetEventRanking URL:  {APIConfig.urladdress}/{APIConfig.userRankingService}/{eventid}/{userid} ");
        if (eventid == null || eventid.Length == 0) { yield break; }

        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();
        webReq.url = string.Format("{0}/{1}/{2}/{3}", APIConfig.urladdress, APIConfig.userRankingService,eventid, userid);
         yield return webReq.SendWebRequest();
        string rawJson = Encoding.Default.GetString(webReq.downloadHandler.data);
        if (data.ContainsKey("GetEventRanking"))
        {
            //Debug.Log($"udapted url {webReq.url} ");
            data["GetEventRanking"] = JSON.Parse(rawJson);
            //Debug.Log($"udapted GetEventRanking dict {data["GetEventRanking"]} raw json {JSON.Parse(rawJson)} ");
        }
        else
            data.Add("GetEventRanking", JSON.Parse(rawJson));

    }

    public IEnumerator UpdateEventRanking(string userid, string eventid, string point)
    {
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();

        webReq.url = string.Format("{0}/{1}/{2}/{3}/{4}", APIConfig.urladdress, APIConfig.userRankingService, userid, eventid, point);
        webReq.method = "put";
        Debug.Log($"update user ranking {webReq.url }");
        // send the web request and wait for a returning result
        yield return webReq.SendWebRequest();
    }
    public IEnumerator UpdateUserNameProfile(string userid, string name)
    {
        //Debug.Log($"UpdateUserNameProfile ");

        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();
        webReq.url = string.Format("{0}/{1}/{2}/{3}", APIConfig.urladdress, APIConfig.userProfileNameService, userid, name);
        webReq.method = "put";
        //Debug.Log($"update user name {webReq.url }");
        yield return webReq.SendWebRequest();
    }
    public IEnumerator UpdateUserReward(string userid, string exp, string gold, string gemtype, string reward)
    {
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();
        webReq.url = string.Format("{0}/{1}/{2}/{3}/{4}/{5}/{6}", APIConfig.urladdress, APIConfig.userRewardService, userid, exp, gold, gemtype, reward);
        webReq.method = "put";
        Debug.Log($"update user reward {webReq.url }");
        yield return webReq.SendWebRequest();
    }
    public IEnumerator UpdateUserLevel(string userid, string level, string exp)
    {
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();
        webReq.url = string.Format("{0}/{1}/{2}/{3}/{4}", APIConfig.urladdress, APIConfig.userLevelService, userid, level, exp);
        webReq.method = "put";
        Debug.Log($"update user level {webReq.url }");
        yield return webReq.SendWebRequest();
    }
    public IEnumerator UpdateGold(string userid, int gold)
    {
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();

        webReq.url = string.Format("{0}/{1}/{2}/{3}", APIConfig.urladdress, APIConfig.userGoldService, userid, gold);
        webReq.method = "put";
        Debug.Log($"update gold {webReq.url }");
        // send the web request and wait for a returning result
        yield return webReq.SendWebRequest();
        
    }
    public IEnumerator UpdateDiamond(string userid, int diamond)
    {
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();

        webReq.url = string.Format("{0}/{1}/{2}/{3}", APIConfig.urladdress, APIConfig.userDiamondService, userid, diamond);
        webReq.method = "put";
        Debug.Log($"update diamond {webReq.url }");
        // send the web request and wait for a returning result
        yield return webReq.SendWebRequest();
    }
    public IEnumerator GetUserProfile(string userid)
    {
        JSONNode jsonResult;
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();
        webReq.url = string.Format("{0}/{1}/{2}", APIConfig.urladdress, APIConfig.userProfileService, userid);
        yield return webReq.SendWebRequest();
        string rawJson = Encoding.Default.GetString(webReq.downloadHandler.data);
        jsonResult = JSON.Parse(rawJson);
        if (jsonResult.Count > 0)
        {
            StaticClass.diamond = jsonResult[0]["diamond"];
            StaticClass.gold = jsonResult[0]["gold"];
            StaticClass.ruby = jsonResult[0]["ruby"];
            StaticClass.opal = jsonResult[0]["opal"];
            StaticClass.sapphire = jsonResult[0]["sapphire"];
            StaticClass.topaz = jsonResult[0]["topaz"];
            StaticClass.emerald = jsonResult[0]["emerald"];
            StaticClass.experience = jsonResult[0]["experience"];
            StaticClass.level = jsonResult[0]["level"];
            StaticClass.highestPoint = jsonResult[0]["highestpoint"];
        }
        else
        {
            StaticClass.diamond = "0";
            StaticClass.gold = "0";
            StaticClass.ruby = "0";
            StaticClass.opal = "0";
            StaticClass.sapphire = "0";
            StaticClass.topaz = "0";
            StaticClass.emerald = "0";
            StaticClass.experience = "0";
            StaticClass.level = "0";
            StaticClass.highestPoint = "0";
        }
        //Debug.Log($"Get User Profile {webReq.url } {jsonResult}");
    }
}
