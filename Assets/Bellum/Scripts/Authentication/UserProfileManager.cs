using System;
using System.Collections;
using System.Text;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;

public class UserProfileManager : MonoBehaviour
{
    [SerializeField] private FirebaseManager firebaseManager;
    public event Action userProfileChanged;
    public event Action requestTextUpdate;

    private void Awake()
    {
        if(firebaseManager!= null)
            firebaseManager.authStateChanged += LoadUserProfile;
    }
    private void OnDestroy()
    {
        if (firebaseManager != null)
            firebaseManager.authStateChanged -= LoadUserProfile;
    }

    public void LoadUserProfile()
    {
        StartCoroutine(GetUserProfile(StaticClass.UserID));
    }
    public IEnumerator GetUserProfile(string userid)
    {
        if (userid == null || userid.Length == 0) {
            StaticClass.diamond = "0";
            StaticClass.gold = "0";
            requestTextUpdate?.Invoke();
            yield break;
        }
        JSONNode jsonResult;
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();
        webReq.url = string.Format("{0}/{1}/{2}", APIConfig.urladdress, APIConfig.userProfileService, userid);
        yield return webReq.SendWebRequest();
        string rawJson = Encoding.Default.GetString(webReq.downloadHandler.data);
        jsonResult = JSON.Parse(rawJson);
        if(jsonResult.Count > 0)
        {
            StaticClass.diamond =  jsonResult[0]["diamond"];
            StaticClass.gold = jsonResult[0]["gold"];
        }
        else
        {
            StaticClass.diamond = "0";
            StaticClass.gold = "0";
        }
        requestTextUpdate?.Invoke();
        //Debug.Log($"Get User Profile {webReq.url } {jsonResult}");
    }

    // IAPShop buy button will call this function when purchase completed.
    public void RewardSignUp()
    {
        RewardDiamond(30);
        RewardGold(2000);
    }
    public void RewardDiamond(int diamond)
    {
        StartCoroutine(UpdateDiamond(StaticClass.UserID, diamond));
    }
    public void RewardGold(int gold)
    {
        StartCoroutine(UpdateGold(StaticClass.UserID, 1000));
    }
    public IEnumerator UpdateGold(string userid, int gold)
    {
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();

        webReq.url = string.Format("{0}/{1}/{2}/{3}", APIConfig.urladdress, APIConfig.userGoldService, userid, gold );
        webReq.method = "put";
        Debug.Log($"update gold {webReq.url }");
        // send the web request and wait for a returning result
        yield return webReq.SendWebRequest();
        userProfileChanged?.Invoke();
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
        userProfileChanged?.Invoke();
    }


}
