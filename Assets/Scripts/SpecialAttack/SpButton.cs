using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpButton : MonoBehaviour
{
   
    [SerializeField] GameObject buttonPrefab;
    public int buttonOffSet;
    public RectTransform FirstCardPos;
    private GameObject button;
    private int buttonCount;
    private List<SpecialAttackDict.SpecialAttackType> spawnedButtonSpType = new List<SpecialAttackDict.SpecialAttackType>();
    private List<GameObject> spawnedSpButton = new List<GameObject>();
    private Sprite sprite;
    void Start()
    {
        
    }
    public bool InstantiateSpButton(SpecialAttackDict.SpecialAttackType spType,Unit unit)
    {
        //only spawn one button for each type of Sp
        if (!spawnedButtonSpType.Contains(spType))
        {
            spawnedButtonSpType.Add(spType);
            buttonCount++;
           
            // spawn the button 
            button = Instantiate(buttonPrefab, transform);
            //Set button pos
            button.GetComponent<RectTransform>().anchoredPosition = new Vector3(FirstCardPos.anchoredPosition.x + buttonOffSet * buttonCount, FirstCardPos.anchoredPosition.y, 0);
            SpecialAttackDict.SpSprite.TryGetValue(spType, out sprite);
            button.GetComponent<Image>().sprite = sprite;
            SpecialAttackDict.ChildSpSprite.TryGetValue(spType, out sprite);
           // button.GetComponentInChildren<Image>().sprite = sprite;
            button.transform.GetChild(0).GetComponent<Image>().sprite = sprite;
            spawnedSpButton.Add(button);
            // tell unit where is the button in the list
            unit.SpBtnTicket = buttonCount - 1;
            return true;
        }
        else
        {
            return false;
        }
        
    }
   
    public GameObject GetButton(int Ticket)
    {
        return spawnedSpButton[Ticket];
    }
    // Update is called once per frame
    void Update()
    {
        
    }
   
}
