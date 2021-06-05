using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TopBarMenu : MonoBehaviour
{
    [SerializeField] public TMP_Text username = null;
    [SerializeField] private UserProfileManager userProfileManager;
    [SerializeField] public TMP_Text gold = null;
    [SerializeField] public TMP_Text diamond = null;
    
    private void Awake()
    {
        userProfileManager.userProfileChanged += HandleProfileUpdate;
        userProfileManager.requestTextUpdate += UserProfileTextUpdate;

    }
    private void OnDestroy()
    {
        userProfileManager.userProfileChanged -= HandleProfileUpdate;
        userProfileManager.requestTextUpdate -= UserProfileTextUpdate;
    }

    public void Start()
    {
        UserProfileTextUpdate();
    }
    public void HandleProfileUpdate()
    {
        StartCoroutine(LoadUserProfile());
    }
    IEnumerator LoadUserProfile()
    {
        yield return userProfileManager.GetUserProfile(StaticClass.UserID);
        UserProfileTextUpdate();
    }
   
    public void UserProfileTextUpdate()
    {
        //Debug.Log($"Top Bar User Profile Text Update");
        username.text = StaticClass.Username;
        gold.text = StaticClass.gold;
        diamond.text = StaticClass.diamond;
    }
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("Scene_Main_Menu");
    }
    public void GoToHeroDeckMenu()
    {
        SceneManager.LoadScene("Scene_Character_Menu");
    }
}
