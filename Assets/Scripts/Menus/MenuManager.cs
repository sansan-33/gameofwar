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
        SceneManager.LoadScene("Scene_hero_Menu");
    }
    public void GoToHeroMenu()
    {
        SceneManager.LoadScene("Scene_Test_Menu");
    }
    public void GoToStoryMenu()
    {
        SceneManager.LoadScene("Scene_Story");
    }
    public void GoToSummonMenu()
    {
        SceneManager.LoadScene("summon");
    }
}
