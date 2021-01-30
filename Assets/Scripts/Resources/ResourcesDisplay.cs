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
    int maxEleixer = 0;
    int currentEleixer;
    private void Start()
    {
      
        // player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        // ClientHandleResourcesUpdated(player.GetResources());

        // player.ClientOnResourcesUpdated += ClientHandleResourcesUpdated;

    }/*

    private void OnDestroy()
    {
        player.ClientOnResourcesUpdated -= ClientHandleResourcesUpdated;
    }

    private void ClientHandleResourcesUpdated(int resources)
    {
        
        int currentresources;
        
        currentresources = player.resources;
        if (currentresources > maxresources)
        {
           
            maxresources = currentresources;
        }
        healthBarImage.fillAmount = (float)currentresources / (float) maxresources;
        Debug.Log($"{currentresources}/{maxresources}");

    }*/
    private void Update()
    {
        GameObject DealManagers = GameObject.FindGameObjectWithTag("DealManager");

        currentEleixer = DealManagers.GetComponent<CardDealer>().eleixer;
        maxEleixer = DealManagers.GetComponent<CardDealer>().maxEleixer;
        healthBarImage.fillAmount = (float)currentEleixer / (float)maxEleixer;
    }
}
