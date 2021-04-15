using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using static CharacterArt;
public class SpButton : MonoBehaviour
{
    [SerializeField] public CharacterArt Arts;
    [SerializeField] GameObject buttonPrefab;
    public int buttonOffSet;
    public RectTransform FirstCardPos;
    private GameObject button;
    private int buttonCount;
    private List<SpecialAttackDict.SpecialAttackType> spawnedButtonSpType = new List<SpecialAttackDict.SpecialAttackType>();
    private List<GameObject> spawnedSpButton = new List<GameObject>();
    private Sprite sprite;
    private RTSPlayer player;
    private GameObject buttonChild;
    void Awake()
    {
        Arts.initDictionary();
    }
    void Strat()
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        CardStats[] units;
        //find all unit
        units = FindObjectsOfType<CardStats>();
        //senemyList = GameObject.FindGameObjectsWithTag("Player" + player.GetEnemyID()).ToList();

        if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)//1 player mode
        {
            foreach (CardStats unit in units)
            {  // Only Set on our side
                if (unit.CompareTag("Player0") || unit.CompareTag("King0"))
                {
                    foreach(SpecialAttackDict.SpecialAttackType type in unit.specialAttackTypes)
                    {
                        InstantiateSpButton(type, unit.GetComponent<Unit>());
                    }
                   
                }
            }

        }
        else // Multi player seneriao
        {
            //Debug.Log($"OnPointerDown Defend SP Multi shieldList {shieldList.Length}");
            foreach (CardStats unit in units)
            {  // Only Set on our side
                if (unit.CompareTag("Player" + player.GetPlayerID()) || unit.CompareTag("King" + player.GetPlayerID()))
                {
                    foreach (SpecialAttackDict.SpecialAttackType type in unit.specialAttackTypes)
                    {
                        InstantiateSpButton(type, unit.GetComponent<Unit>());
                    }
                }
            }

        }
    }
    public void InstantiateSpButton(SpecialAttackDict.SpecialAttackType spType,Unit unit)
    {
        //only spawn one button for each type of Sp
        
            
            buttonCount++;
          // if(spType == SpecialAttackDict.SpecialAttackType.Shield) { Debug.Log(buttonCount); }
            // spawn the button
            CharacterImage characterImage = Arts.CharacterArtDictionary[unit.unitKey.ToString()];
            button = Instantiate(buttonPrefab, transform);
            //Set button pos
            button.GetComponent<RectTransform>().anchoredPosition = new Vector3(FirstCardPos.anchoredPosition.x + buttonOffSet * buttonCount, FirstCardPos.anchoredPosition.y, 0);
            buttonChild = button.transform.Find("mask").gameObject;
            buttonChild.transform.GetChild(0).GetComponent<Image>().sprite = characterImage.image;
            SpecialAttackDict.ChildSpSprite.TryGetValue(spType, out sprite);
            buttonChild.transform.GetChild(1).GetComponent<Image>().sprite = sprite;
            spawnedSpButton.Add(button);
            // tell unit where is the button in the list
     
        
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
