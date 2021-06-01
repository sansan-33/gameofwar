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
        data.Add("GetTotalPower", JSON.Parse(rawJson));

    }
    public IEnumerator GetTeamInfo(string userid)
    {
        if (userid == null || userid.Length == 0) { yield break; }

        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();
        webReq.url = string.Format("{0}/{1}/{2}", APIConfig.urladdress, APIConfig.teamService, userid);
        yield return webReq.SendWebRequest();
        string rawJson = Encoding.Default.GetString(webReq.downloadHandler.data);
        data.Add("GetTeamInfo", JSON.Parse(rawJson));
    }
}
