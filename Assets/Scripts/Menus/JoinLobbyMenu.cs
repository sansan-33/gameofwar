using System;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinLobbyMenu : MonoBehaviour
{
    [SerializeField] private GameObject landingPagePanel = null;
    [SerializeField] private TMP_InputField addressInput = null;
    [SerializeField] private Button joinButton = null;

    private void OnEnable()
    {
        RTSNetworkManager.ClientOnConnected += HandleClientConnected;
        RTSNetworkManager.ClientOnDisconnected += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        RTSNetworkManager.ClientOnConnected -= HandleClientConnected;
        RTSNetworkManager.ClientOnDisconnected -= HandleClientDisconnected;
    }
    public void Join()
    {
        string address = addressInput.text;
        int port = 7777;
        if (address.Contains(":"))
        {
            port = Int32.Parse(address.Split(':')[1]);
            address = address.Split(':')[0];
        }
        NetworkManager.singleton.networkAddress = address;
        NetworkManager.singleton.StartClient(GetUri(address , port));

        joinButton.interactable = false;
    }
    public Uri GetUri(string address, int port)
    {
        UriBuilder builder = new UriBuilder();
        builder.Scheme = "tcp4";
        builder.Host = address;
        builder.Port = port;
        return builder.Uri;
    }

    private void HandleClientConnected()
    {
        joinButton.interactable = true;

        gameObject.SetActive(false);
        landingPagePanel.SetActive(false);
    }

    private void HandleClientDisconnected()
    {
        joinButton.interactable = true;
    }

   
}
