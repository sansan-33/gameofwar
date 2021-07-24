using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TopBarMenu : MonoBehaviour
{
    [SerializeField] public TMP_Text username = null;
    [SerializeField] private UserProfileManager userProfileManager;
    APIManager apiManager;
    [SerializeField] public TMP_Text gold = null;
    [SerializeField] public TMP_Text diamond = null;
    [SerializeField] private Image userExperienceSlider = null;
    [SerializeField] private TMP_Text userExperienceText = null;
    [SerializeField] public TMP_Text userLevel = null;


    private void Awake()
    {
        userProfileManager.userProfileChanged += HandleProfileUpdate;
        userProfileManager.requestTextUpdate += UserProfileTextUpdate;
        FirebaseManager.userNameChanged += UserProfileTextUpdate; 

    }
    private void OnDestroy()
    {
        userProfileManager.userProfileChanged -= HandleProfileUpdate;
        userProfileManager.requestTextUpdate -= UserProfileTextUpdate;
        FirebaseManager.userNameChanged -= UserProfileTextUpdate;
    }

    public void Start()
    {
        apiManager = new APIManager();
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
        Debug.Log($"Top Bar User Profile Text Update");
        username.text = StaticClass.Username;
        gold.text = StaticClass.gold;
        diamond.text = StaticClass.diamond;
        StartCoroutine(userLevelUp());
    }
    IEnumerator userLevelUp()
    {
        if (userExperienceText != null)
        {
            float fillVal = (float)(Int32.Parse(StaticClass.experience) / (100f * (Int32.Parse(StaticClass.level) + 1)));
            userExperienceText.text = StaticClass.experience;
            userExperienceSlider.fillAmount = fillVal;
            userLevel.text = StaticClass.level;
            if (fillVal > 1) {
                yield return apiManager.UpdateUserLevel(StaticClass.UserID, StaticClass.level, StaticClass.experience);
                yield return LoadUserProfile();
            }
        }
    }
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("Scene_Main_Menu");
    }
    public void GoToHeroDeckMenu()
    {
        SceneManager.LoadScene("Scene_Character_Menu");
    }
    public void GoToStageMenu()
    {
        SceneManager.LoadScene("Scene_Stage_Menu");
    }
}
