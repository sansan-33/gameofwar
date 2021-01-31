using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System;
using System.Linq;
using SimpleJSON;
using TMPro;

public class MatchMaking : MonoBehaviour
{

    private const float API_CHECK_MAXTIME = 10 * 60.0f; //10 minutes
    public GameObject addressPanel;
    public GameObject serverIP;
    private float apiCheckCountdown = API_CHECK_MAXTIME;

    // API url
    private string url = "https://www.schoolapis.com:443/schools/";
    private int schoolid = 5200;
    // resulting JSON from an API request
    private JSONNode jsonResult;


    void Start()
    {
        //CheckLobbyStatus();
    }
    void Update()
    {
        //apiCheckCountdown -= Time.deltaTime;
        //if (apiCheckCountdown <= 0)
        //{
        //    CheckLobbyStatus();
        //    apiCheckCountdown = API_CHECK_MAXTIME;
        //}
    }
    public void CheckLobbyStatus()
    {
        addressPanel.SetActive(true);
        // get and set the data
        StartCoroutine(GetData(serverIP.GetComponent<TMP_InputField>()));
        
    }
    // sends an API request - returns a JSON file
    IEnumerator GetData(TMP_InputField tmp_text)
    {
        Debug.Log($"address text {tmp_text.text}");
        // create the web request and download handler
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();

        // build the url and query
        webReq.url = string.Format("{0}{1}", url, schoolid);

        // send the web request and wait for a returning result
        yield return webReq.SendWebRequest();

        // convert the byte array to a string
        string rawJson = Encoding.Default.GetString(webReq.downloadHandler.data);

        // parse the raw string into a json result we can easily read
        jsonResult = JSON.Parse(rawJson);
        Debug.Log($"jsonResult {webReq.url } {jsonResult["namehk"]}");
        // display the results on screen
        tmp_text.text =  jsonResult["namehk"];
    }



}
