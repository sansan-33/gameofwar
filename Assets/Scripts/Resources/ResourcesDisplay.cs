using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourcesDisplay : MonoBehaviour
{
    [SerializeField] private Image healthBarImage = null;
    int maxEleixer = 0;
    int currentEleixer;
    private eleixier Eleixier;

    private void Start()
    {
        Eleixier = FindObjectOfType<eleixier>();
        maxEleixer = Eleixier.maxEleixer;
    }
    private void Update()
    {
        currentEleixer = Eleixier.eleixer;
        healthBarImage.fillAmount = (float)currentEleixer / (float)maxEleixer;
        
    }
}
