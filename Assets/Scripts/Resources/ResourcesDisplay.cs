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

        if (Screen.height <= 2500 && Screen.width <= 1500)
        {

            

            RectTransform rt = this.GetComponent<RectTransform>();


            rt.anchoredPosition = new Vector3(-177, 30, 0);
            rt.sizeDelta = new Vector2(300, (float)22.5);
        }
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
