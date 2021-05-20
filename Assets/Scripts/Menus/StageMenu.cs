using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageMenu : MonoBehaviour
{
    private void Start()
    {
        RTSNetworkManager.ClientOnConnected += HandleClientConnected;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated += AuthorityHandlePartyOwnerStateUpdated;
        RTSPlayer.ClientOnInfoUpdated += ClientHandleInfoUpdated;
        Mirror.NetworkManager.singleton.StartHost();
        StaticClass.Chapter = "1";
    }

    private void OnDestroy()
    {
        RTSNetworkManager.ClientOnConnected -= HandleClientConnected;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated -= AuthorityHandlePartyOwnerStateUpdated;
        RTSPlayer.ClientOnInfoUpdated -= ClientHandleInfoUpdated;
        LeaveLobby();
    }

    private void HandleClientConnected()
    {
    }

    private void ClientHandleInfoUpdated()
    {
        NetworkClient.connection.identity.GetComponent<RTSPlayer>().CmdSetUserID(StaticClass.UserID);
        List<RTSPlayer> players = ((RTSNetworkManager)NetworkManager.singleton).Players;
    }

    private void AuthorityHandlePartyOwnerStateUpdated(bool state)
    {
        
    }
    public void LeaveLobby()
    {
        if (StaticClass.Chapter != null && StaticClass.Mission != null ) { return; }

        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.offlineScene = "Scene_Main_Menu";
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();
            SceneManager.LoadScene("Scene_Main_Menu");
        }
    }
}
