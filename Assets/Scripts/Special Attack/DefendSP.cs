using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DefendSP : MonoBehaviour
{
    private SpCost spCost;

    public float SPCost = 10;
    public int shieldHealths = 100;
    private Button SPButton;
    void Start()
    {
        spCost = FindObjectOfType<SpCost>();
        SPButton = GameObject.FindGameObjectWithTag("SpDefend").GetComponent<Button>();
        SPButton.onClick.RemoveAllListeners();
        SPButton.onClick.AddListener(OnPointerDown);
    }

    public void OnPointerDown()
    {
        //if(SPAmount < SPCost) {return;}
        spCost.SPAmount -= (int)SPCost;
        GameObject[] shieldList;
        shieldList = GameObject.FindGameObjectsWithTag("Shield");
        foreach (GameObject shield in shieldList)
        {
            if (shield.transform.parent.CompareTag("Player0")|| shield.transform.parent.CompareTag("King0"))
            {
                shield.GetComponent<Shield>().shieldHealth = shieldHealths;

            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
