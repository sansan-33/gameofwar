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
    private eleixier Eleixier;

    private void Start()
    {
        eleixier.UpdateEleixer += UpdateEleixer;
        Eleixier = FindObjectOfType<eleixier>();
        maxEleixer = Eleixier.maxEleixer;
    }
    private void UpdateEleixer(int eleixer)
    {
        healthBarImage.fillAmount = (float)eleixer / (float)maxEleixer;   
    }
    private void OnDestroy()
    {
        eleixier.UpdateEleixer -= UpdateEleixer;
    }
}
