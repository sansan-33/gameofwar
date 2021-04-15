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
    private ISpecialAttack SpecialAttack;
    void Awake()
    {
        Arts.initDictionary();
    }
    
    private void Start()
    {
        StartCoroutine(start());
    }
    private IEnumerator start()
    {
        yield return new WaitForSeconds(2);
    player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
    CardStats[] units;
    //find all unit
    units = FindObjectsOfType<CardStats>();
    //senemyList = GameObject.FindGameObjectsWithTag("Player" + player.GetEnemyID()).ToList();
    Debug.Log($"{units.Length}");
    if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)//1 player mode
    {

        foreach (CardStats unit in units)
        {  // Only Set on our side
            Debug.Log($"one player mode {unit.tag}");
            if (unit.CompareTag("Player0") || unit.CompareTag("King0"))
            {
                Debug.Log("one player mode");
                SpecialAttack = unit.GetComponent(typeof(ISpecialAttack)) as ISpecialAttack;
                InstantiateSpButton(unit.specialAttackTypes, unit.GetComponent<Unit>(), SpecialAttack);
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

                SpecialAttack = unit.GetComponent(typeof(ISpecialAttack)) as ISpecialAttack;
                InstantiateSpButton(unit.specialAttackTypes, unit.GetComponent<Unit>(), SpecialAttack);

            }
        }
    }
    }
    public void InstantiateSpButton(SpecialAttackDict.SpecialAttackType spType,Unit unit, ISpecialAttack specialAttack)
    {
        //only spawn one button for each type of Sp
        Debug.Log($"spawn {spType}");
            
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
            button.GetComponent<Button>().onClick.AddListener(specialAttack.OnPointerDown);
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
