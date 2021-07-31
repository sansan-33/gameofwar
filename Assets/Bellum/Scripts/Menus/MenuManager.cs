using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private FirebaseManager firebaseManager = null;
    [SerializeField] public GameObject unitPreviewParent;

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("Scene_Lobby");
    }
    public void GoToHeroDeckMenu()
    {
        SceneManager.LoadScene("Scene_Character_Menu");
    }
    public void GoToHeroMenu()
    {
        SceneManager.LoadScene("Scene_Hero_Menu");
    }
    public void GoToStoryMenu()
    {
        SceneManager.LoadScene("Scene_Story");
    }
    public void GoToSummonMenu()
    {
        SceneManager.LoadScene("Scene_Summon");
    }
    public void GoToTeamMenu()
    {
        SceneManager.LoadScene("Scene_Team_Menu");
    }
    public void GoToStageMenu()
    {
        SceneManager.LoadScene("Scene_Stage_Menu");
    }
    public void GoToStageHumanDetailMenu()
    {
        SceneManager.LoadScene("Scene_Stage_Human");
    }
    public void GoToLogin()
    {
        if(unitPreviewParent != null ) unitPreviewParent.SetActive(false);
        firebaseManager.ShowLoginProfile();
    }
    public void GoToShop()
    {
        SceneManager.LoadScene("Scene_Shop");
    }
    public void GoToRanking()
    {
        SceneManager.LoadScene("Scene_Ranking");
    }
    public void GoToAchievement()
    {
        SceneManager.LoadScene("Scene_Achievement");
    }
}
