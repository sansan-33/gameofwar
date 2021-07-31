using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStartManager : MonoBehaviour
{
    public void GoToLobby()
    {
        SceneManager.LoadScene("Scene_Lobby");
    }
}
