using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("Scene_Main_Menu");
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
        SceneManager.LoadScene("summon");
    }
    public void GoToTeamMenu()
    {
        SceneManager.LoadScene("Scene_Team_Menu");
    }
}
