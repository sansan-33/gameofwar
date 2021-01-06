using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourcesDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text resourcesText = null;
    [SerializeField] private Image healthBarImage = null;
    private RTSPlayer player;
    int maxresources = 0;

    private void Start()
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        ClientHandleResourcesUpdated(player.GetResources());

        player.ClientOnResourcesUpdated += ClientHandleResourcesUpdated;
    }

    private void OnDestroy()
    {
        player.ClientOnResourcesUpdated -= ClientHandleResourcesUpdated;
    }

    private void ClientHandleResourcesUpdated(int resources)
    {
        int currentresources;
        
        currentresources = resources;
        if (currentresources > maxresources)
        {
           
            maxresources = currentresources;
        }
        healthBarImage.fillAmount = (float)currentresources / (float) maxresources;

    }
}
