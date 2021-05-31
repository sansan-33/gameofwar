using System;
using System.Collections;
using System.Text;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;

public class APIManager 
{
    public JSONNode data;
    // sends an API request - returns a JSON file
    public IEnumerator GetTotalPower(string userid, string cardkeys)
    {
        if (userid == null || userid.Length == 0) { yield break; }
        JSONNode jsonResult;
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();
        webReq.url = string.Format("{0}/{1}/{2}/{3}", APIConfig.urladdress, APIConfig.cardService, userid, cardkeys);
        yield return webReq.SendWebRequest();
        string rawJson = Encoding.Default.GetString(webReq.downloadHandler.data);
        jsonResult = JSON.Parse(rawJson);
        data = jsonResult;

    }
     
}
