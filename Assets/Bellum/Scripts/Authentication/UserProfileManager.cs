using System;
using System.Collections;
using System.Text;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class UserProfileManager : MonoBehaviour
{
    [SerializeField] private FirebaseManager firebaseManager;
    public event Action userProfileChanged;
    public event Action requestTextUpdate;
    APIManager apiManager;
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
    public void Start()
    {
        apiManager = new APIManager();
    }
    public void LoadUserProfile()
    {
        StartCoroutine(GetUserProfile(StaticClass.UserID));
    }
   
    public IEnumerator GetUserProfile(string userid)
    {
        if (userid == null || userid.Length == 0)
        {
            StaticClass.diamond = "0";
            StaticClass.gold = "0";
            requestTextUpdate?.Invoke();
            yield break;
        }
        yield return apiManager.GetUserProfile(userid);
        requestTextUpdate?.Invoke();
    }
    // IAPShop buy button will call this function when purchase completed.
    public void RewardSignUp()
    {
        RewardDiamond(30);
        RewardGold(2000);
    }
    public void RewardDiamond(int diamond)
    {
        StartCoroutine(updateDiamond(diamond));
    }
    public IEnumerator updateDiamond(int diamond)
    {
        yield return apiManager.UpdateDiamond(StaticClass.UserID, diamond);
        userProfileChanged?.Invoke();
    }
    public void RewardGold(int gold)
    {
        StartCoroutine(updateGold(gold));
    }
    IEnumerator updateGold(int gold)
    {
        yield return apiManager.UpdateGold(StaticClass.UserID, gold);
        userProfileChanged?.Invoke();
    }


}
