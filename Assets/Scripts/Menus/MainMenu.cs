using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject landingPagePanel = null;
    [SerializeField] public TMP_Text userid = null;

    public void Start()
    {
        if(StaticClass.UserID == null || StaticClass.UserID.Length == 0)
            StaticClass.UserID = "1";

        userid.text = StaticClass.UserID;
    }
    public void HostLobby()
    {
        landingPagePanel.SetActive(false);

        NetworkManager.singleton.StartHost();
    }
}
