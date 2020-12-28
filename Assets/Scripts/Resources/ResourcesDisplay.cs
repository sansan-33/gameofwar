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
        int maxresources = 10;
        currentresources = resources;
      //  resourcesText.text = $"Resources: {resources}";
        healthBarImage.fillAmount = (float)currentresources / maxresources;
        Debug.Log($"healthBarImage.fillAmount");
    }
}
