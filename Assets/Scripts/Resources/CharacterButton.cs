using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterButton : MonoBehaviour
{

    public Button buttonComponent;
   
    // Use this for initialization
    void Start()
    {
        buttonComponent.onClick.RemoveAllListeners();
        buttonComponent.onClick.AddListener(HandleClick);
    }

    public void HandleClick()
    {
        SceneManager.LoadScene("Scene_Hero_Menu");
    }
}
