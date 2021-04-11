using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System;
using System.Linq;
using SimpleJSON;
using TMPro;
using System.Net;

public class MatchMaking : MonoBehaviour
{
    public GameObject addressPanel;
    public GameObject serverIP;

    // API url
    private string status = "ready";
    private string limit = "1";
    // resulting JSON from an API request
    private JSONNode jsonResult;
  
    public void CheckLobbyStatus()
    {
        addressPanel.SetActive(true);
        //StartCoroutine(GetReadyServer(serverIP.GetComponent<TMP_InputField>()));
        StartCoroutine(GetLocalServer(serverIP.GetComponent<TMP_InputField>()));
    }
    public void HandleQuitGame(string gameserverport)
    {
        Debug.Log($"HandleQuitGame {gameserverport}");
        StartCoroutine(QuitGameServer(gameserverport));
    }
    // sends an API request - returns a JSON file
    IEnumerator GetLocalServer(TMP_InputField tmp_text)
    {
        tmp_text.text = GetLocalIPv4() + ":7777";
        yield return null;
    }
    // sends an API request - returns a JSON file
    IEnumerator GetReadyServer(TMP_InputField tmp_text)
    {
        Debug.Log($"address text {tmp_text.text}");
        // create the web request and download handler
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();

        // build the url and query
        webReq.url = string.Format("{0}/{1}/{2}/{3}", APIConfig.urladdress, APIConfig.gameServerService, status, limit);

        // send the web request and wait for a returning result
        yield return webReq.SendWebRequest();

        // convert the byte array to a string
        string rawJson = Encoding.Default.GetString(webReq.downloadHandler.data);

        // parse the raw string into a json result we can easily read
        jsonResult = JSON.Parse(rawJson);
        Debug.Log($"jsonResult {webReq.url } {jsonResult["port"]}");
        // display the results on screen
        //gameserverport = jsonResult["port"];
        tmp_text.text = jsonResult["serverip"] + ":" + jsonResult["port"];
       
    }
    IEnumerator QuitGameServer(string port)
    {
        Debug.Log($"QuitGameServer {port}");
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();

        // build the url and query
        webReq.url = string.Format("{0}/{1}/{2}", APIConfig.urladdress, APIConfig.quitService, port);
        webReq.method = "put";
        Debug.Log($"QuitGameServer {webReq.url }");
        // send the web request and wait for a returning result
        yield return webReq.SendWebRequest();

        // convert the byte array to a string
        string rawJson = Encoding.Default.GetString(webReq.downloadHandler.data);
    }
    public string GetLocalIPv4()
    {
        return Dns.GetHostEntry(Dns.GetHostName())
            .AddressList.First(
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .ToString();
    }

}
